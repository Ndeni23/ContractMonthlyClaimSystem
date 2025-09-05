namespace ContractMonthlyClaimSystem.Models
{
    public class Lecturer : User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal HourlyRate { get; set; }
    }
}