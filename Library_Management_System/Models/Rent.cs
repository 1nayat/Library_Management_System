namespace Library_Management_System.Models
{
    public class Rent
    {
        public int RentID { get; set; }
        public int BookID { get; set; }
        public int UserID { get; set; }
        public int Days { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int Status { get; set; }
        public string? BookName { get; set; }
        public string ?StudentName { get; set; }
        public Book? Book { get; set; }
    }
}
