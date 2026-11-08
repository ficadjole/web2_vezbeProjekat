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

        public async Task<Result<PurchaseResponseDTO>> GetPurchase(Guid idempotencyKey)
        {
            if(idempotencyKey == Guid.Empty)
            {
                return Result.Failure<PurchaseResponseDTO>("Idempotency key is empty.");
            }

            var purchesOrcherProxy = ServiceProxy.Create<IPurchaseOrchestrator>(new Uri("fabric:/ProjekatVezbeWeb/PurchaseOrchestrator"), new ServicePartitionKey(PartitionKeyHelper.ForTransaction(idempotencyKey)));

            var purchaseResponse = await purchesOrcherProxy.GetPurchaseAsync(idempotencyKey);

            if (purchaseResponse == null)
            {
                return Result.Failure<PurchaseResponseDTO>("Purchase not found.");
            }

            return Result.Success(purchaseResponse);

        }

        public async Task<ValidationResultDto> PurchaseCencalltion(Guid idempotencyKey)
        {
            if (idempotencyKey == Guid.Empty)
            {
                return new ValidationResultDto
                {
                    IsValid = false,
                    Message = "Idempotency key is empty."
                };
            }
        
            var purchesOrcherProxy = ServiceProxy.Create<IPurchaseOrchestrator>(new Uri("fabric:/ProjekatVezbeWeb/PurchaseOrchestrator"), new ServicePartitionKey(PartitionKeyHelper.ForTransaction(idempotencyKey)));

            var cancellationResult = await purchesOrcherProxy.AddCancelRequest(idempotencyKey);

            if (!cancellationResult)
            {
                return new ValidationResultDto
                {
                    IsValid = false,
                    Message = "Purchase cancellation could not be completed."
                };
            }

            return new ValidationResultDto
            {
                IsValid = true,
                Message = "Purchase cancellation completed successfully."
            };

        }


        public async Task<ValidationResultDto> ValidateAsync(BuyBookRequestDTO buyBookRequestDTO, Guid idempotencyKey)
        {

            if(buyBookRequestDTO.UserId == string.Empty || buyBookRequestDTO.BookId == string.Empty)
            {
                return new ValidationResultDto
                {
                    IsValid = false,
                    Message = "UserId or Book is null."
                };
            }

            if(buyBookRequestDTO.Quantity <= 0)
            {
                return new ValidationResultDto
                {
                    IsValid = false,
                    Message = "Quantity must be greater than zero."
                };
            }

            var purchesOrcherProxy = ServiceProxy.Create<IPurchaseOrchestrator>(new Uri("fabric:/ProjekatVezbeWeb/PurchaseOrchestrator"), new ServicePartitionKey(PartitionKeyHelper.ForTransaction(idempotencyKey)));

            var purchaseResult = await purchesOrcherProxy.StartPurchaseAsync(idempotencyKey, buyBookRequestDTO.UserId, buyBookRequestDTO.BookId, buyBookRequestDTO.Quantity);

            if (purchaseResult == Guid.Empty)
            {
                return new ValidationResultDto
                {
                    IsValid = false,
                    Message = "Purchase could not be completed."
                };
            }

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
