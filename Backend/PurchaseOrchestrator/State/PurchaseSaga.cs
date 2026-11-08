using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PurchaseOrchestrator.State
{


    [DataContract]
    public sealed record PurchaseSaga
    {
        [DataMember]
        public Guid PurchaseId { get; init; }
        [DataMember]
        public string UserId { get; init; } = string.Empty;
        [DataMember]
        public string BookId { get; init; } = string.Empty;
        [DataMember] 
        public double Price { get; init; }

        [DataMember]
        public int Quantity { get; init; }

        [DataMember]
        public PurchaseStatus Status { get; init; }
        [DataMember]
        public int Attempt { get; init; }
        [DataMember]
        public DateTime NextAttemptUTC { get; init; }
        [DataMember]
        public string? LastError { get; init; }
        [DataMember]
        public long Version { get; init; }


        public bool IsTerminal => Status is PurchaseStatus.Completed or PurchaseStatus.Compensated or PurchaseStatus.Failed;
        public bool PastPivot => Status is PurchaseStatus.Confirming or PurchaseStatus.Completed;

        public PurchaseSaga ScheduleRetry(string error) => this with
        {
            Attempt = this.Attempt + 1,
            LastError = error,
            NextAttemptUTC = DateTime.UtcNow +
            TimeSpan.FromSeconds(Math.Min(60, Math.Pow(2, Attempt))) +
            TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000))
        };

        public PurchaseSaga StartCompensation(string reason) => this with
        {
            Status = PurchaseStatus.Compensating,
            Attempt = 0,
            LastError = reason,
            NextAttemptUTC = DateTime.UtcNow
        };
    }
}
