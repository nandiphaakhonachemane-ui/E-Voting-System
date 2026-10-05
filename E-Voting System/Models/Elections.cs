using Microsoft.AspNetCore.Mvc;

namespace E_Voting_System.Models
{
    public class Election
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;          // e.g. "2029 National & Provincial Elections"
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsOpen { get; set; }
        public bool ResultsPublished { get; set; }

        public ICollection<VotingToken> VotingTokens { get; set; } = new List<VotingToken>();
        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }
}