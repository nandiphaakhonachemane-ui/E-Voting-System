using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Voting_System.Models
{
    public class Vote
    {
        public long Id { get; set; }   // instead of int

        public int ElectionId { get; set; }

        public int PartyId { get; set; }                    // National party

        public int? ProvincialPartyId { get; set; }         // Provincial party (optional)

        [Required]
        [StringLength(20)]
        public string BallotType { get; set; } = "National";

        public DateTime CastAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(ElectionId))]
        public Election? Election { get; set; }

        [ForeignKey(nameof(PartyId))]
        public Party? Party { get; set; }

        [ForeignKey(nameof(ProvincialPartyId))]
        public Party? ProvincialParty { get; set; }
    }
}