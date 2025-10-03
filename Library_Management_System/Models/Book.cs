namespace Library_Management_System.Models
{
    public class Book
    {
        public int BookID { get; set; }
        public string? BookName { get; set; }
        public string? Author { get; set; }
        public string? Detail { get; set; }
        public double Price { get; set; }
        public string? Publication { get; set; }
        public string? Branch { get; set; }
        public int Quantities { get; set; }
        public int AvailableQnt { get; set; }
        public int RentQnt { get; set; }
        public string? Image { get; set; }
        public string? BookPDF { get; set; }
        public DateTime EntryDate { get; set; }
        public int AvailableCopies { get; set; } // new

    }
}
