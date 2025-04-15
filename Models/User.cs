using System.ComponentModel.DataAnnotations;

namespace IBanKing.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public DateTime DateBirth { get; set; }

        public string Gender { get; set; }

        public string Address { get; set; }

        public string PhoneNumber { get; set; }

        public string Role { get; set; }

        public bool IsBlocked { get; set; }
        public int FailedLoginAttempts { get; set; } = 0;
    }
}
