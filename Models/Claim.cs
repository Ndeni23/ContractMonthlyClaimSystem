using ContractMonthlyClaimSystem.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractMonthlyClaimSystem.Models
{
    public class Claim
    {
        [Key]
        public int ClaimId { get; set; }

        public DateTime SubmissionDate { get; set; }
        public decimal HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; }
        public string? Comments { get; set; }

        // --- Explicit Foreign Key Relationship ---
        public int LecturerId { get; set; }
        [ForeignKey("LecturerId")]
        public virtual User? Lecturer { get; set; }

        public virtual ICollection<SupportingDocument>? SupportingDocuments { get; set; }
    }
}

