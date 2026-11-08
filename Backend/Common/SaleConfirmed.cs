using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public sealed record SaleConfirmed
    {
        public SaleConfirmed(Guid purchaseId, string bookId, double price, DateTime occurredAt)
        {
            PurchaseId = purchaseId;
            BookId = bookId;
            Price = price;
            OccurredAt = occurredAt;
        }

        [DataMember]
        public Guid PurchaseId { get; init; }
        [DataMember] public string BookId { get; init; } = string.Empty;
        [DataMember] public double Price { get; init; }
        [DataMember] public DateTime OccurredAt { get; init; }
    }
}
