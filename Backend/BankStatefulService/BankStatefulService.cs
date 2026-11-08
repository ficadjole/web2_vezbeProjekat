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
using BankStatefulService.Exceptions;

namespace BankStatefulService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class BankStatefulService : StatefulService, IBankService
    {
        public BankStatefulService(StatefulServiceContext context)
            : base(context)
        { }

        public async Task<bool> WithdrawAsync(string accountId, double amount)
        {
            var bankDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, BankAccount>>("bank");

            using (var tx = this.StateManager.CreateTransaction())
            {
                var result = bankDictionary.TryGetValueAsync(tx, accountId);

                if (result.Result.HasValue && result.Result.Value.Balance >= amount)
                {
                    var account = result.Result.Value;
                    account.Balance -= amount;

                    //Ako kljuc postoji azuriraj vrednost, sa prosledjenim accountom, ako ne postoji dodaj novi element sa prosledjenim accountom
                    //await bankDictionary.AddOrUpdateAsync(tx, accountId, account, (key, value) => account);

                    await bankDictionary.TryUpdateAsync(tx, accountId, account, result.Result.Value);

                    await tx.CommitAsync();

                    return true;
                }


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

        private async Task AddData()
        {
            var bankDictionary = this.StateManager.GetOrAddAsync<IReliableDictionary<string, BankAccount>>("bank");
            using (var tx = this.StateManager.CreateTransaction())
            {
                await bankDictionary.Result.AddAsync(tx, "1", new BankAccount { Id = "1", Owner = "Pera Peric", Balance = 1000 });
                await bankDictionary.Result.AddAsync(tx, "2", new BankAccount { Id = "2", Owner = "Misa Misic", Balance = 2000 });
                await bankDictionary.Result.AddAsync(tx, "3", new BankAccount { Id = "3", Owner = "Nikolaj Romanov", Balance = 3000 });
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
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

            //await AddData();
            await myDictionary.ClearAsync();
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

        public Task<long> GetCountAsync()
        {
            var bankDictionary = this.StateManager.GetOrAddAsync<IReliableDictionary<string, BankAccount>>("bank");
            using (var tx = this.StateManager.CreateTransaction())
            {
                var count = bankDictionary.Result.GetCountAsync(tx);
                return count;

            }
        }

        public async Task<bool> AddAccount(BankAccount account)
        {
            var bankDictionary = this.StateManager.GetOrAddAsync<IReliableDictionary<string, BankAccount>>("bank");
            using (var tx = this.StateManager.CreateTransaction())
            {
                var result = await bankDictionary.Result.TryAddAsync(tx, account.Id, account);
                await tx.CommitAsync();
                return result;

            }
        }

        public async Task<List<BankAccount>> GetAllAccountsAsync()
        {
            var bankDictionary = this.StateManager.GetOrAddAsync<IReliableDictionary<string, BankAccount>>("bank");
            using (var tx = this.StateManager.CreateTransaction())
            {
                var accounts = new List<BankAccount>();

                var enumerator = await bankDictionary.Result.CreateEnumerableAsync(tx);

                var enumeratorResult = enumerator.GetAsyncEnumerator();

                while (await enumeratorResult.MoveNextAsync(CancellationToken.None))
                {
                    var current = enumeratorResult.Current;
                    accounts.Add(current.Value);
                }
                await tx.CommitAsync();

                return accounts;
            }
        }

        public async Task ChargeAsync(Guid TxId, string userId, double amount)
        {

            var bankDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, BankAccount>>("bank");

            var charges = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, Charge>>("charges");

            using (var tx = this.StateManager.CreateTransaction()) {


                var existing = await charges.TryGetValueAsync(tx, TxId, LockMode.Update);

                if (existing.HasValue)
                    return;

                var acc = await bankDictionary.TryGetValueAsync(tx,userId, LockMode.Update);

                if (!acc.HasValue)
                    throw new AccountNotFoundException($"Account for userID: {userId} is not found");

                if (acc.Value.Balance < amount)
                    throw new InsufficientFundsException($"Account: {userId} does not have enough funds");

                acc.Value.Balance -= amount;

                await bankDictionary.SetAsync(tx, userId, acc.Value);

                await charges.AddAsync(tx, TxId, new Charge(TxId, userId, amount, ChargeStatus.Charged ));

                await tx.CommitAsync();

               // await Task.Delay(10000); // Simulate some processing time

            }
        }

        public async Task  RefundAsync(Guid TxId, string userId)
        {
            var bankDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, BankAccount>>("bank");

            var charges = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, Charge>>("charges");

            using (var tx = this.StateManager.CreateTransaction())
            {

                var existing = await charges.TryGetValueAsync(tx, TxId, LockMode.Update);

                if (!existing.HasValue)
                {
                    await charges.TryAddAsync(tx, TxId, new Charge(TxId, userId, 0, ChargeStatus.Refunded));
                }
                else if(existing.Value.status == ChargeStatus.Charged)
                {

                    var acc = await bankDictionary.TryGetValueAsync(tx, userId, LockMode.Update);

                    if (!acc.HasValue)
                        throw new AccountNotFoundException($"Account for userID: {userId} is not found");


                    acc.Value.Balance += existing.Value.amount;

                    await bankDictionary.SetAsync(tx, userId, acc.Value);

                    await charges.SetAsync(tx, TxId, existing.Value with { status = ChargeStatus.Refunded});

                }

                await tx.CommitAsync();

            }
        }

        public async Task<BankAccount> GetAccount(string accountId)
        {
            var bankDictionary = this.StateManager.GetOrAddAsync<IReliableDictionary<string, BankAccount>>("bank");

            using (var tx = this.StateManager.CreateTransaction())
            {
                var result = await bankDictionary.Result.TryGetValueAsync(tx, accountId);
                if (result.HasValue)
                {
                    return result.Value;
                }
                else
                {
                    throw new AccountNotFoundException($"Account with ID: {accountId} is not found");
                }
            }
        }
    }
}
