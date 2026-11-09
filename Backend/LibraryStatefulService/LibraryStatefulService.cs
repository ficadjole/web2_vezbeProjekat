using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Common;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace LibraryStatefulService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class LibraryStatefulService : StatefulService, ILibraryService
    {
        public LibraryStatefulService(StatefulServiceContext context)
            : base(context)
        { }

        public async Task<Book> BookAvailableAsync(string id,int quantity)
        {
            var libraryDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Book>>("library");

            using(var tx = this.StateManager.CreateTransaction())
            {
                var result = await libraryDictionary.TryGetValueAsync(tx, id);

                if (result.HasValue && result.Value.Quantity >= quantity) {

                    var book = result.Value;
                     //book.Quantity -= quantity;

                    await libraryDictionary.TryUpdateAsync(tx, id, book, result.Value);
                    await tx.CommitAsync();

                    return book;

                }
                await tx.CommitAsync();
                return new Book();
                

            }
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        private async Task AddData()
        {
            var libraryDictionary = this.StateManager.GetOrAddAsync<IReliableDictionary<string, Book>>("library");

            using(var tx = this.StateManager.CreateTransaction())
            {
               await libraryDictionary.Result.AddAsync(tx, "1", new Book { Id = "1", Title = "Dekaton: Pocetak rata", Author = "Vladimir Martic",Price = 2000,Quantity = 2, Reserved =0 });
                await libraryDictionary.Result.AddAsync(tx, "2", new Book { Id = "2", Title = "Crni obelisk", Author = "C.M. Remark", Price = 1899, Quantity = 3 , Reserved = 0});
                await libraryDictionary.Result.AddAsync(tx, "3", new Book { Id = "3", Title = "20000 milja pod morem", Author = "Zil Vern", Price = 1000, Quantity = 1, Reserved = 0 });
               await tx.CommitAsync();
            }
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            var outbox = await this.StateManager.GetOrAddAsync<IReliableQueue<SaleConfirmed>>("outbox");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    using (var tx = this.StateManager.CreateTransaction())
                    {


                        if (outbox != null)
                        {

                            var peekResult = await outbox.TryPeekAsync(tx);

                            if (peekResult.HasValue)
                            {
                                var saleConfirmed = peekResult.Value;

                                ServiceEventSource.Current.Message($"Sale confirmed: {saleConfirmed.BookId}, Price: {saleConfirmed.Price}, Timestamp: {saleConfirmed.OccurredAt}");

                                var dequeueResult = await outbox.TryDequeueAsync(tx);

                                if (dequeueResult.HasValue)
                                    await tx.CommitAsync();

                            }

                        }

                    }
                }
                catch(Exception ex)
                {
                    ServiceEventSource.Current.Message($"Error in RunAsync: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        public async Task<long> GetCountAsync()
        {
            var libraryDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Book>>("library");
            using (var tx = this.StateManager.CreateTransaction())
            {
                var count = await libraryDictionary.GetCountAsync(tx);
                return count;
            }
        }

        public async Task<bool> AddBook(Book book)
        {
            var libraryDictionary = this.StateManager.GetOrAddAsync<IReliableDictionary<string, Book>>("library");

            using (var tx = this.StateManager.CreateTransaction())
            {
                var result = await libraryDictionary.Result.TryAddAsync(tx, book.Id, book);
                await tx.CommitAsync();
                return result;
            }
        }

        public async Task<List<Book>> GetAllBooksAsync()
        {
            var libraryDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Book>>("library");

            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumerator = await libraryDictionary.CreateEnumerableAsync(tx);
                var enumeratorResult = enumerator.GetAsyncEnumerator();
                var books = new List<Book>();
                while (await enumeratorResult.MoveNextAsync(CancellationToken.None))
                {
                    books.Add(enumeratorResult.Current.Value);
                }

                await tx.CommitAsync();
                return books;
            }
        }


        public async Task<ConsistencyCheckDTO> ConsistencyCheck(string bookId)
        {
            var books = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Book>>("library");
            var reservations = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, Reservation>>("reservations");

            using (var tx = this.StateManager.CreateTransaction())
            {
                var book = await books.TryGetValueAsync(tx, bookId, LockMode.Update);
                if (!book.HasValue) throw new BookNotFoundException($"Book with ID: {bookId} not found");
                var reservationList = await reservations.CreateEnumerableAsync(tx);
                var enumerator = reservationList.GetAsyncEnumerator();
                int reservedCount = 0;
                while (await enumerator.MoveNextAsync(CancellationToken.None))
                {
                    if (enumerator.Current.Value.BookId == bookId && enumerator.Current.Value.Status == ReservationStatus.Reserved)
                    {
                        reservedCount += enumerator.Current.Value.Quantity;
                    }
                }



                bool isConsistent = (book.Value.Reserved == reservedCount);

                await tx.CommitAsync();

                return new ConsistencyCheckDTO
                {
                    Quantity = book.Value.Quantity,
                    Reserved = book.Value.Reserved,
                    nmbOfReservations = reservedCount,
                    ReservationsQuantitySum = book.Value.Reserved + book.Value.Quantity,
                    IsConsistent = isConsistent
                };

            }
        }

        public async Task<double> ReserveAsync(Guid txId, string bookId, int quantity)
        {
            var books = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Book>>("library");
            var reservations = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, Reservation>>("reservations");

            using (var tx = StateManager.CreateTransaction())
            {

                var reservationsResult = await reservations.TryGetValueAsync(tx, txId, LockMode.Update);

                if (reservationsResult.HasValue)
                {
                    if (reservationsResult.Value.Status == ReservationStatus.Reserved)
                    {
                        throw new InvalidOperationException($"Reservation with ID: {txId} is already reserved");
                    }

                    var book = await books.TryGetValueAsync(tx, reservationsResult.Value.BookId, LockMode.Update);

                    return book.HasValue ? book.Value.Price : 0;
                }

                var bookResult = await books.TryGetValueAsync(tx, bookId, LockMode.Update);

                if (!bookResult.HasValue)
                    throw new BookNotFoundException($"Book with ID: {bookId} not found");

                if (bookResult.Value.Quantity < quantity) throw new OutOfStockException($"Book with ID: {bookId} is out of stock");

                await books.SetAsync(tx, bookId, bookResult.Value with
                {
                    Quantity = bookResult.Value.Quantity - quantity,
                    Reserved = bookResult.Value.Reserved + quantity
                });

                await reservations.AddAsync(tx, txId, new Reservation
                (
                    txId,
                    bookId,
                    quantity,
                    (decimal)bookResult.Value.Price,
                     ReservationStatus.Reserved
                ));

                await tx.CommitAsync();
                return bookResult.Value.Price;
            }
        }


        public async Task ConfirmAsync(Guid txId)
        {

            var reservations = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, Reservation>>("reservations");
            var books = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Book>>("library");
            var outbox = await this.StateManager.GetOrAddAsync<IReliableQueue<SaleConfirmed>>("outbox");

            using(var tx = this.StateManager.CreateTransaction())
            {
                var reservationResult = await reservations.TryGetValueAsync(tx, txId, LockMode.Update);

                if (!reservationResult.HasValue || reservationResult.Value.Status != ReservationStatus.Reserved)
                    return;

                var bookResult = await books.TryGetValueAsync(tx, reservationResult.Value.BookId, LockMode.Update);

                await books.SetAsync(tx,reservationResult.Value.BookId, bookResult.Value with { Reserved = bookResult.Value.Reserved - reservationResult.Value.Quantity });

                await reservations.SetAsync(tx, txId, reservationResult.Value with { Status = ReservationStatus.Confirmed });

                await outbox.EnqueueAsync(tx, new SaleConfirmed(txId, reservationResult.Value.BookId, bookResult.Value.Price, DateTime.UtcNow));

                await tx.CommitAsync();
            }

        }

        public async Task ReleaseaAsync(Guid txId)
        {
            var books = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Book>>("library");
            var reservations = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, Reservation>>("reservations");

            using(var tx = StateManager.CreateTransaction())
            {
                var reservationResult = await reservations.TryGetValueAsync(tx, txId);

                if (!reservationResult.HasValue)
                {
                    await reservations.TryAddAsync(tx,txId, new Reservation(txId,"",0,0,ReservationStatus.Released));
                }
                else if (reservationResult.Value.Status == ReservationStatus.Reserved)
                {

                    var bookResult = await books.TryGetValueAsync(tx, reservationResult.Value.BookId);

                    if (!bookResult.HasValue)
                        throw new BookNotFoundException($"Book with ID: {reservationResult.Value.BookId} not found");

                    await books.SetAsync(tx, bookResult.Value.Id, bookResult.Value with { 
                        Quantity = bookResult.Value.Quantity + reservationResult.Value.Quantity, 
                        Reserved = bookResult.Value.Reserved - reservationResult.Value.Quantity 
                    });

                    await reservations.SetAsync(tx, txId, reservationResult.Value with { Status = ReservationStatus.Released });
                }

                await tx.CommitAsync();
            }
        }

        public async Task<Book> GetBook(string bookId)
        {
            var libraryDictionary = this.StateManager.GetOrAddAsync<IReliableDictionary<string, Book>>("library");

            using (var tx = this.StateManager.CreateTransaction())
            {
                var result = await libraryDictionary.Result.TryGetValueAsync(tx, bookId);
                if (result.HasValue)
                {
                    return result.Value;
                }
                else
                {
                    throw new BookNotFoundException($"Book with ID: {bookId} not found");
                }
            }
        }
    }
}
