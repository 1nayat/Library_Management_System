using System;
using System.ComponentModel.DataAnnotations;

namespace Library_Management_System.Models
{
    public class User
    {
        public int Id { get; set; }

  
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string? Name { get; set; }


        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(256)]
        public string Email { get; set; } = null!;


        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        public string PasswordHash { get; set; } = null!;

        public string Role { get; set; } = "Student";

        public string? Image { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
