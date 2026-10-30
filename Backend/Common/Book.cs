using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public sealed record Book
    {
        public Book()
        {
        }

        public Book(string id, string title, string author, double price ,int quantity, int reserved)
        {
            Id = id;
            Title = title;
            Author = author;
            Price = price;
            Quantity = quantity;
            Reserved = reserved;
        }
        [DataMember]
        public string Id { get; init; } = string.Empty;
        [DataMember]
        public string Title { get; init; } = string.Empty;
        [DataMember]
        public string Author { get; init; } = string.Empty;
        [DataMember]
        public double Price { get; init; }
        [DataMember]
        public int Quantity { get; init; }
        [DataMember]
        public int Reserved { get; init; }



    }
}
