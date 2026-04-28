namespace Common
{
    public class Book
    {
        public Book()
        {
        }

        public Book(int id, string title, string author, double price ,int quantity)
        {
            Id = id;
            Title = title;
            Author = author;
            Price = price;
            Quantity = quantity;
        }

        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Quantity { get; set; }



    }
}
