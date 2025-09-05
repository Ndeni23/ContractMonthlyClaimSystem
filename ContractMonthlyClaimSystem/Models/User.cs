namespace ContractMonthlyClaimSystem.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; } // For later, not used in prototype
        public string Role { get; set; }
    }
}