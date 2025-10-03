namespace Library_Management_System.Models
{
    public class Penalty
    {
        public int PenaltyID { get; set; }
        public int UserID { get; set; }
        public int BookID { get; set; }
        public string? BookName { get; set; }
        public double Price { get; set; }
        public double PenaltyAmount { get; set; }
        public string? Detail { get; set; }
        public DateTime EntryDate { get; set; }
        public string? StudentName { get; set; }
        public bool IsPaid { get; set; }
        public Rent? Rent { get; set; } // optional: to show which book

    }
}
