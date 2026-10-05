using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace E_Voting_System.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(13, MinimumLength = 13)]
        public string SouthAfricanId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        public string? Ward { get; set; }

        public bool IsEligibleVoter { get; set; } = false;

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    }
}