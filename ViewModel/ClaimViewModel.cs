using System.ComponentModel.DataAnnotations;
using System;

namespace ContractMonthlyClaimSystem.ViewModel
{
    public class ClaimViewModel
    {
        public int ClaimId { get; set; }

        [Required(ErrorMessage = "Please enter the module name.")]
        [StringLength(100)]
        [Display(Name = "Description / Module Name")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Hours Worked is required.")]
        [Range(0.1, 100, ErrorMessage = "Hours must be between 0.1 and 100.")]
        [Display(Name = "Hours Worked")]
        public decimal HoursWorked { get; set; }

        [Required(ErrorMessage = "Hourly Rate is required.")]
        [Range(10, 1000, ErrorMessage = "Hourly Rate must be between 10 and 1000.")]
        [Display(Name = "Hourly Rate")]
        public decimal HourlyRate { get; set; }

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        public string? Status { get; set; }
        public DateTime SubmissionDate { get; set; }

        // --- NEW HELPER METHOD FOR TESTING ---
        public decimal CalculateTotal()
        {
            return HoursWorked * HourlyRate;
        }
    }
}