using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractMonthlyClaimSystem.Models
{
    public class Claim
    {
        [Key] // Specifies this is the primary key
        public int ClaimId { get; set; }

        public DateTime SubmissionDate { get; set; }
        public decimal HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } // e.g., "Submitted", "Pre-approved", "Approved", "Rejected"
        public string? Comments { get; set; }

        // --- Foreign Key for Lecturer ---
        public int LecturerId { get; set; }
        [ForeignKey("LecturerId")]
        public virtual Lecturer Lecturer { get; set; }

        // --- Navigation property for documents ---
        public virtual ICollection<SupportingDocument> SupportingDocuments { get; set; }
    }
}