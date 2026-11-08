using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Common;
using PurchaseOrchestrator.State;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Client;

namespace PurchaseOrchestrator
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class PurchaseOrchestrator : StatefulService, IPurchaseOrchestrator
    {

        private const int MaxAttemps = 3;
        public PurchaseOrchestrator(StatefulServiceContext context)
            : base(context)
        {


        }

        public async Task<Guid> StartPurchaseAsync(Guid purchaseId, string userId, string bookId, int quantity)
        {
            var sagas = await StateManager.GetOrAddAsync<IReliableDictionary<Guid, PurchaseSaga>>("purchases");

            using (var tx = StateManager.CreateTransaction())
            {

                var added = await sagas.TryAddAsync(tx, purchaseId, new PurchaseSaga
                {
                    PurchaseId = purchaseId,
                    UserId = userId,
                    BookId = bookId,
                    Quantity = quantity,
                    Status = PurchaseStatus.Started,
                    NextAttemptUTC = DateTime.UtcNow
                });

                if (!added)
                {
                    var existing = await sagas.TryGetValueAsync(tx, purchaseId);

                    if (existing.Value.UserId != userId || existing.Value.BookId != bookId)
                    {
                        throw new IdempotencyKeyConflictException($"Purchase with ID {purchaseId} already exists with different user or book.");
                    }
                }

                await tx.CommitAsync();
                return purchaseId;

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

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            var sagas = await StateManager.GetOrAddAsync<IReliableDictionary<Guid, PurchaseSaga>>("purchases");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach(var saga in await LoadDueAsync(sagas, cancellationToken))
                {
                    await AdvanceAsync(sagas, saga, cancellationToken);
                }
                await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
            }


        }

        private async Task<List<PurchaseSaga>> LoadDueAsync(IReliableDictionary<Guid, PurchaseSaga> sagas, CancellationToken ct)
        {
            var due = new List<PurchaseSaga>();

            using (var tx = StateManager.CreateTransaction())
            {
                var enumerable = await sagas.CreateEnumerableAsync(tx);
                using (var e = enumerable.GetAsyncEnumerator())
                {
                    while (await e.MoveNextAsync(ct))
                    {
                        if (!e.Current.Value.IsTerminal && e.Current.Value.NextAttemptUTC <= DateTime.UtcNow)
                        {
                            due.Add(e.Current.Value);
                        }
                    }
                }
            }

            return due;
        }
        #region Private Methods for Advancing the Saga
        private async Task AdvanceAsync(IReliableDictionary<Guid, PurchaseSaga> sagas, PurchaseSaga saga, CancellationToken ct)
        {
            PurchaseSaga next;
            try
            {

                if (!saga.PastPivot && saga.Status != PurchaseStatus.Compensating && await IsCancelRequestedAsync(saga.PurchaseId))
                {
                    next = saga.StartCompensation("Cancellation requested");
                }
                else
                {
                    next = saga.Status switch
                    {
                        PurchaseStatus.Started => saga with { Status = PurchaseStatus.BookReserved, Attempt = 0, Price = await Library(saga.BookId).ReserveAsync(saga.PurchaseId, saga.BookId, saga.Quantity) },
                        PurchaseStatus.BookReserved => await ChargeAsync(saga),
                        PurchaseStatus.AccountCharged => await ConfirmAsync(saga with { Status = PurchaseStatus.Confirming }), //PIVOT
                        PurchaseStatus.Confirming => await ConfirmAsync(saga),
                        PurchaseStatus.Compensating => await CompensateAsync(saga),
                        _ => saga
                    };
                }

            }
            catch (Exception ex) when (IsTransient(ex) && saga.Attempt < MaxAttemps)
            {
                next = saga.ScheduleRetry(ex.Message);
            }
            catch (Exception ex)
            {
                next = saga switch
                {
                    { Status: PurchaseStatus.Compensating }
                    => saga with { Status = PurchaseStatus.Failed, LastError = ex.Message }, { PastPivot: true }
                    => saga.ScheduleRetry(ex.Message),
                    _ => saga.StartCompensation(ex.Message)
                };
            }

            using (var tx = StateManager.CreateTransaction())
            {
                var current = await sagas.TryGetValueAsync(tx, saga.PurchaseId, LockMode.Update);

                if(current.HasValue && current.Value.Version == saga.Version)
                {
                    await sagas.SetAsync(tx, saga.PurchaseId, next with { Version = saga.Version + 1});
                    await tx.CommitAsync();
                }
            }
        }

        private bool IsTransient(Exception ex)
        {
            return ex is TimeoutException ||
                   ex is FabricException fabricEx && (
                       fabricEx.ErrorCode == FabricErrorCode.OperationTimedOut ||
                       fabricEx.ErrorCode == FabricErrorCode.ReconfigurationPending) ||
                   ex.InnerException is TimeoutException;
        }


        private async Task<PurchaseSaga> ChargeAsync(PurchaseSaga saga)
        {
            await Bank(saga.UserId).ChargeAsync(saga.PurchaseId, saga.UserId, saga.Price);
            return saga with { Status = PurchaseStatus.AccountCharged, Attempt = 0 };
        }

        private async Task<PurchaseSaga> ConfirmAsync(PurchaseSaga saga)
        {
            await Library(saga.BookId).ConfirmAsync(saga.PurchaseId);
            return saga with { Status = PurchaseStatus.Completed, Attempt = 0 };
        }

        private async Task<PurchaseSaga> CompensateAsync(PurchaseSaga saga)
        {

            await Bank(saga.UserId).RefundAsync(saga.PurchaseId, saga.UserId);
            await Library(saga.BookId).ReleaseaAsync(saga.PurchaseId);
            return saga with { Status = PurchaseStatus.Compensated };
        }

        #endregion

        #region Service Proxies
        private IBankService Bank(string userId)
        {
            return ServiceProxy.Create<IBankService>(new Uri("fabric:/ProjekatVezbeWeb/BankStatefulService"), new ServicePartitionKey(PartitionKeyHelper.ForAccount(userId)));
        }

        private ILibraryService Library(string bookId)
        {
            return ServiceProxy.Create<ILibraryService>(new Uri("fabric:/ProjekatVezbeWeb/LibraryStatefulService"), new ServicePartitionKey(PartitionKeyHelper.ForBook(bookId)));
        }

        #endregion

        public async Task<bool> AddCancelRequest(Guid purchaseId)
        {
            var sagas = await StateManager.GetOrAddAsync<IReliableDictionary<Guid, PurchaseSaga>>("purchases");
            var cancelRequests = await StateManager.GetOrAddAsync<IReliableDictionary<Guid, bool>>("cancelRequests");

            using (var tx = StateManager.CreateTransaction())
            {

                var saga = await sagas.TryGetValueAsync(tx, purchaseId);

                if (!saga.HasValue)
                {
                    throw new InvalidOperationException($"Purchase with ID {purchaseId} does not exist.");
                }

                if(saga.Value.PastPivot)
                {
                    return false;
                }

                await cancelRequests.SetAsync(tx, purchaseId, true);
                await tx.CommitAsync();
            }
            
            return true;
        }

        private async Task<bool> IsCancelRequestedAsync(Guid purchaseId)
        {
            var cancelRequests = await StateManager.GetOrAddAsync<IReliableDictionary<Guid, bool>>("cancelRequests");
            using (var tx = StateManager.CreateTransaction())
            {

                var result = await cancelRequests.TryGetValueAsync(tx, purchaseId);
                return result.HasValue && result.Value;
            }
        }

        public async Task<PurchaseResponseDTO> GetPurchaseAsync(Guid purchaseId)
        {
            var sagas = await StateManager.GetOrAddAsync<IReliableDictionary<Guid, PurchaseSaga>>("purchases");

            using (var tx = StateManager.CreateTransaction())
            {
                var sagaResult = await sagas.TryGetValueAsync(tx, purchaseId);

                if(!sagaResult.HasValue)
                {
                    throw new InvalidOperationException($"Purchase with ID {purchaseId} does not exist.");
                }

                var saga = sagaResult.Value;

                return new PurchaseResponseDTO
                {
                    Status = saga.Status,
                    Price = saga.Price,
                    Attempt = saga.Attempt,
                    LastError = saga.LastError ?? string.Empty
                };

            }

        }
    }
}
