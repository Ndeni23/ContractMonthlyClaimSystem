using System;
using System.Collections.Generic;

namespace ContractMonthlyClaimSystem.Models
{
    public class Claim
    {
        public int ClaimId { get; set; }
        public DateTime SubmissionDate { get; set; }
        public decimal HoursWorked { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string? Comments { get; set; } // <-- ADD '?' HERE

        public int LecturerId { get; set; }
    }
}