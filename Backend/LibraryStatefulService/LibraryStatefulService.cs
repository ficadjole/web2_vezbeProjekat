using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        public async Task<bool> BookAvailableAsync(int id,int quantity)
        {
            var libraryDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<int, Book>>("library");

            using(var tx = this.StateManager.CreateTransaction())
            {
                var result = await libraryDictionary.TryGetValueAsync(tx, id);
    
                

                if (result.HasValue && result.Value.Quantity >= quantity) {

                    var book = result.Value;
                     book.Quantity -= quantity;

                    await libraryDictionary.TryUpdateAsync(tx, id, book, result.Value);
                    await tx.CommitAsync();

                    return true;

                }
                await tx.CommitAsync();
                return false;
                

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

        private void AddData()
        {
            var libraryDictionary = this.StateManager.GetOrAddAsync<IReliableDictionary<int, Book>>("library");

            using(var tx = this.StateManager.CreateTransaction())
            {
                libraryDictionary.Result.AddAsync(tx, 1, new Book { Id = 1, Title = "Dekaton: Pocetak rata", Author = "Vladimir Martic",Price = 2000,Quantity = 2 });
                libraryDictionary.Result.AddAsync(tx, 2, new Book { Id = 2, Title = "Crni obelisk", Author = "C.M. Remark", Price = 1899,Quantity = 3 });
                libraryDictionary.Result.AddAsync(tx, 3, new Book { Id = 3, Title = "20000 milja pod morem", Author = "Zil Vern",Price = 1000 ,Quantity = 1 });
                tx.CommitAsync();
            }
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

            AddData();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
