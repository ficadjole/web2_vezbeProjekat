using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface IBankService : IService
    {
        Task<bool> WithdrawAsync(string accountId, double amount);
        Task<long> GetCountAsync();

        Task<bool> AddAccount(BankAccount account);

        Task<List<BankAccount>> GetAllAccountsAsync();
    }
}
