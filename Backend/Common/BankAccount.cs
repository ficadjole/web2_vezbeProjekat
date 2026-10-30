using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class BankAccount
    {
        public BankAccount()
        {
        }

        public BankAccount(string id, string owner, double balance)
        {
            Id = id;
            Owner = owner;
            Balance = balance;
        }
        [DataMember]
        public string Id { get; set; } = string.Empty;
        [DataMember]
        public string Owner { get; set; } = string.Empty;

        [DataMember] 
        public double Balance { get; set; }
    }
}
