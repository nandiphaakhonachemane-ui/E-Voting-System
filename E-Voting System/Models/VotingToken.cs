using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Voting_System.Models
{
    public class VotingToken
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int ElectionId { get; set; }

        [Required]
        [StringLength(100)]
        public string Token { get; set; } = string.Empty;

        public bool IsUsed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UsedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        [ForeignKey(nameof(ElectionId))]
        public Election? Election { get; set; }
    }
}