namespace Library_Management_System.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = "Student"; 
        public string? Image { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
