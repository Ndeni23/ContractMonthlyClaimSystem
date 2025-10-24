using System.ComponentModel.DataAnnotations;

namespace ContractMonthlyClaimSystem.ViewModel
{
    public class RegisterViewModel
    {
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Email Address")]
        public string Email { get; set; }

        public string Role { get; set; }

        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}