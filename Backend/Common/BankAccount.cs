using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class BankAccount
    {
        public BankAccount()
        {
        }

        public BankAccount(int id, string owner, double balance)
        {
            Id = id;
            Owner = owner;
            Balance = balance;
        }

        public int Id { get; set; }
        public string Owner { get; set; } = string.Empty;
        public double Balance { get; set; }
    }
}
