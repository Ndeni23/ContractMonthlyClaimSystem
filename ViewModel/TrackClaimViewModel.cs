using System;

namespace ContractMonthlyClaimSystem.ViewModel
{
    public class TrackClaimViewModel
    {
        public int UserId { get; set; }
        public int ModuleId { get; set; }
        public int Sessions { get; set; }
        public int Hours { get; set; }
        public decimal Rate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } // "Pending", "Pre-approved", "Approved"
        public DateTime? SubmittedDateTime { get; set; }
        public DateTime? PreApprovedDateTime { get; set; }
        public DateTime? FinalizedDateTime { get; set; }
        public string DocumentName { get; set; }
    }
}