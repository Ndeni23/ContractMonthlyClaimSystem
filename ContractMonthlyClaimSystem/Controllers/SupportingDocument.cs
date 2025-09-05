using System;

namespace ContractMonthlyClaimSystem.Models
{
    public class SupportingDocument
    {
        public int DocumentId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadDate { get; set; }

        // Foreign Key to link to the Claim
        public int ClaimId { get; set; }
    }
}