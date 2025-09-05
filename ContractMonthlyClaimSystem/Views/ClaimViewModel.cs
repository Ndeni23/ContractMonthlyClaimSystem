using System;
using System.ComponentModel.DataAnnotations; // Add this for display attributes

namespace ContractMonthlyClaimSystem.ViewModels
{
    public class ClaimViewModel
    {
        public int ClaimId { get; set; }
        public DateTime SubmissionDate { get; set; }

        [Required] // Makes the field mandatory
        public string Description { get; set; }

        public string Status { get; set; }

        [Required]
        [Display(Name = "Total Amount")] // This text will appear as the label in the form
        public decimal TotalAmount { get; set; }
    }
}