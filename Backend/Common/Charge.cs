using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public sealed record Charge
    {
        public Charge(Guid txId, string userId, double amount, ChargeStatus status)
        {
            TxId = txId;
            this.userId = userId;
            this.amount = amount;
            this.status = status;
        }

        [DataMember]
        public Guid TxId { get; init; }
        [DataMember]
        public string userId { get; init; } = string.Empty;
        [DataMember]
        public double amount {  get; init; }
        [DataMember]
        public ChargeStatus status { get; init; }

    }
}
