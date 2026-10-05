using System.ComponentModel.DataAnnotations;

namespace E_Voting_System.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(13, MinimumLength = 13)]
        [Display(Name = "South African ID Number")]
        public string SouthAfricanId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Address")]
        public string? Address { get; set; }

        public string? Ward { get; set; }
    }
}


