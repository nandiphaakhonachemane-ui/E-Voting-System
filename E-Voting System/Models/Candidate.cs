using Microsoft.AspNetCore.Mvc;


namespace E_Voting_System.Models
{
    public class Candidate
    {
        public int Id { get; set; }
        public int PartyId { get; set; }
        public Party Party { get; set; } = null!;

        public string FullName { get; set; } = string.Empty;
        public string? Position { get; set; }          // e.g. "National List #1"
        public string BallotType { get; set; } = "National"; // National / Provincial
        public string? Province { get; set; }          // null for national
        public int ListPosition { get; set; }
    }
}
