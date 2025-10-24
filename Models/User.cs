using System.ComponentModel.DataAnnotations;

namespace ContractMonthlyClaimSystem.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } // We will store a hashed password here
        public string Role { get; set; } // "Lecturer", "Programme Coordinator", etc.
    }
}