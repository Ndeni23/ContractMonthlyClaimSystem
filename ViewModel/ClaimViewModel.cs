using System.ComponentModel.DataAnnotations;

namespace ContractMonthlyClaimSystem.ViewModel
{
    public class ClaimViewModel
    {
        public int ClaimId { get; set; }

        [Display(Name = "Description / Module Name")]
        public string Description { get; set; }

        [Display(Name = "Hours Worked")]
        public decimal HoursWorked { get; set; }

        [Display(Name = "Hourly Rate")]
        public decimal HourlyRate { get; set; }

        public string? Status { get; set; }
        public DateTime SubmissionDate { get; set; }
    }
}