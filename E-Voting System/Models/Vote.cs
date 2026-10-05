using Microsoft.AspNetCore.Mvc;

namespace E_Voting_System.Models
{
    public class Vote
    {
        public long Id { get; set; }

        public int ElectionId { get; set; }
        public Election Election { get; set; } = null!;

        public int PartyId { get; set; }                     // National ballot
        public Party Party { get; set; } = null!;

        public int? ProvincialPartyId { get; set; }          // Provincial ballot (optional)
        public Party? ProvincialParty { get; set; }

        public string BallotType { get; set; } = "National"; // or "Both"

        // No UserId here – this is the secrecy guarantee
        public DateTime CastAt { get; set; } = DateTime.UtcNow;

        // Optional encrypted payload if you want extra protection
        public string? EncryptedPayload { get; set; }
    }
}
