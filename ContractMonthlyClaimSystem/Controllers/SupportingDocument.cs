using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractMonthlyClaimSystem.Models
{
    public class SupportingDocument
    {
        [Key]
        public int DocumentId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; } // Path on the server where the file is stored

        // --- Foreign Key for Claim ---
        public int ClaimId { get; set; }
        [ForeignKey("ClaimId")]
        public virtual Claim Claim { get; set; }
    }
}