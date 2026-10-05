using Microsoft.AspNetCore.Mvc;

namespace E_Voting_System.Models
{
    public class VotingToken
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public int ElectionId { get; set; }
        public Election Election { get; set; } = null!;

        public string Token { get; set; } = string.Empty;       // GUID or crypto-random
        public bool IsUsed { get; set; } = false;
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UsedAt { get; set; }
    }
}
