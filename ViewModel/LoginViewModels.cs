using System.ComponentModel.DataAnnotations;

namespace ContractMonthlyClaimSystem.ViewModel
{
    public class LoginViewModel
    {
        // Validation attributes have been removed to comply with prototype requirements.
        [Display(Name = "Email (username)")]
        public string Email { get; set; }

        public string Role { get; set; }

        public string Password { get; set; }
    }
}