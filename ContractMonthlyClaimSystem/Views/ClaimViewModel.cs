using System;
using System.ComponentModel.DataAnnotations;

namespace ContractMonthlyClaimSystem.ViewModels
{
    public class ClaimViewModel
    {
        public int ClaimId { get; set; }
        public DateTime SubmissionDate { get; set; }

        [Required]
        public string Description { get; set; }

        public string? Status { get; set; } // <-- ADD '?' HERE

        [Required]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }
    }
}