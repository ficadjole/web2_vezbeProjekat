using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public sealed record Reservation
    {
        public Reservation(Guid txId, string bookId, int quantity, decimal price, ReservationStatus status)
        {
            TxId = txId;
            BookId = bookId;
            Quantity = quantity;
            Price = price;
            Status = status;
        }

        [DataMember] public Guid TxId { get; init; } = Guid.Empty;
        [DataMember] public string BookId { get; init; } = string.Empty;
        [DataMember] public int Quantity { get; init; }
        [DataMember] public decimal Price { get; init; }
        [DataMember] public ReservationStatus Status { get; init; } = ReservationStatus.Reserved;

    }
}
