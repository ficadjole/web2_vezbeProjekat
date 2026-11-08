using System.Runtime.Serialization;


namespace Common
{
    [DataContract]
    public record PurchaseResponseDTO
    {
        [DataMember]
        public PurchaseStatus Status { get; init; }

        [DataMember]
        public double Price { get; init; }
        [DataMember]
        public int Attempt { get; init; }
        [DataMember]
        public string LastError { get; init; } = string.Empty;
    }
}
