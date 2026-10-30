using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace ValidatorStatelessService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class ValidatorStatelessService : StatelessService, IValidator
    {
        private FabricClient fabricClient = new FabricClient();

        public ValidatorStatelessService(StatelessServiceContext context)
            : base(context)
        { 
        }

   

        public async Task<IEnumerable<DiagnosticResultDto>> DiagnosticAsync()
        {
            var partitionsForLibrary = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/ProjekatVezbeWeb/LibraryStatefulService"));
            var partitionsForBank = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/ProjekatVezbeWeb/BankStatefulService"));

            var diagnosticResults = new List<DiagnosticResultDto>();

            foreach (var partition in partitionsForLibrary) {

                var partitionId = partition.PartitionInformation.Id.ToString();
                var int64Info = partition.PartitionInformation as Int64RangePartitionInformation;
                if(int64Info == null)
                {
                    throw new InvalidOperationException("Partition information is not of type Int64RangePartitionInformation.");
                }
                var lowKey = int64Info.LowKey;
                var highKey = int64Info.HighKey;
                var proxy = ServiceProxy.Create<ILibraryService>(new Uri("fabric:/ProjekatVezbeWeb/LibraryStatefulService"), new ServicePartitionKey(lowKey));
                var count = await proxy.GetCountAsync();

                diagnosticResults.Add(new DiagnosticResultDto
                {
                    ServiceName = "LibraryStatefulService",
                    PartitionId = partitionId,
                    PartitionKey = highKey,
                    LowKey = lowKey,
                    GetCount = count
                });
            }

            foreach (var partition in partitionsForBank)
            {
                var partitionId = partition.PartitionInformation.Id.ToString();
                var int64Info = partition.PartitionInformation as Int64RangePartitionInformation;
                if (int64Info == null)
                {
                    throw new InvalidOperationException("Partition information is not of type Int64RangePartitionInformation.");
                }
                var lowKey = int64Info.LowKey;
                var highKey = int64Info.HighKey;
                var proxy = ServiceProxy.Create<IBankService>(new Uri("fabric:/ProjekatVezbeWeb/BankStatefulService"), new ServicePartitionKey(lowKey));
                var count = await proxy.GetCountAsync();

                diagnosticResults.Add(new DiagnosticResultDto
                {
                    ServiceName = "BankStatefulService",
                    PartitionId = partitionId,
                    PartitionKey = highKey,
                    LowKey = lowKey,
                    GetCount = count
                });
            }

            return diagnosticResults;

        }

        public async Task<List<BankAccount>> GetAllAccountsAsync()
        {
            var paritionsForBank = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/ProjekatVezbeWeb/BankStatefulService"));

            var calls = paritionsForBank.Select(p =>
            {
                long lowKey = ((Int64RangePartitionInformation)p.PartitionInformation).LowKey;
                var proxy = ServiceProxy.Create<IBankService>(new Uri("fabric:/ProjekatVezbeWeb/BankStatefulService"), new ServicePartitionKey(lowKey));
                return proxy.GetAllAccountsAsync();
            });

            var results = await Task.WhenAll(calls);
            return results.SelectMany(r => r).OrderBy(b => b.Owner).ToList();
        }

        public async Task<List<Book>> GetAllBooksAsync()
        {
            var paritionsForLibrary =await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/ProjekatVezbeWeb/LibraryStatefulService"));

            var calls = paritionsForLibrary.Select(p =>
            {
                long lowKey = ((Int64RangePartitionInformation)p.PartitionInformation).LowKey;
                var proxy = ServiceProxy.Create<ILibraryService>(new Uri("fabric:/ProjekatVezbeWeb/LibraryStatefulService"), new ServicePartitionKey(lowKey));
                return proxy.GetAllBooksAsync();
            });

            var results = await Task.WhenAll(calls);

            return results.SelectMany(r => r).OrderBy(b => b.Title).ToList();
        }

        public async Task<ValidationResultDto> ValidateAsync(BuyBookRequestDTO buyBookRequestDTO)
        {

            if (buyBookRequestDTO.Book.Title.Length < 5)
            {
                return new ValidationResultDto
                {
                    IsValid = false,
                    Message = "Title must be at least 5 characters long."
                };
            }

            if (buyBookRequestDTO.Book.Author.Length < 5)
            {
                return new ValidationResultDto
                {
                    IsValid = false,
                    Message = "Author must be at least 5 characters long."
                };
            }

            if (buyBookRequestDTO.Book.Quantity <= 0)
            {
                return new ValidationResultDto
                {
                    IsValid = false,
                    Message = "Quantity cannot be negative or zero."
                };
            }

            var proxy = ServiceProxy.Create<ILibraryService>(new Uri("fabric:/ProjekatVezbeWeb/LibraryStatefulService"), new ServicePartitionKey(0));

            var result = await proxy.BookAvailableAsync(buyBookRequestDTO.Book.Id,buyBookRequestDTO.Book.Quantity);

            if (result.Id == "")
            {
                    return new ValidationResultDto
                    {
                        IsValid = false,
                        Message = "Book with the given ID is not available in the library."
                    };
            }

            var bankProxy = ServiceProxy.Create<IBankService>(new Uri("fabric:/ProjekatVezbeWeb/BankStatefulService"), new ServicePartitionKey(0));

            var paymentResult = await bankProxy.WithdrawAsync(buyBookRequestDTO.UserId, result.Price); //UserID == BankAccountID

            if (!paymentResult)
            {
                return new ValidationResultDto
                {
                    IsValid = false,
                    Message = "Payment failed. Insufficient funds."
                };
            }

            var mailingProxy = ServiceProxy.Create<IMailingService>(new Uri("fabric:/ProjekatVezbeWeb/MailingStatefulService"), new ServicePartitionKey(0));

            await mailingProxy.PublishEvent(buyBookRequestDTO);

            return new ValidationResultDto
            {
                IsValid = true,
                Message = "Book is valid."
            };

        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners();
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
