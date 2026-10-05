using System.ComponentModel.DataAnnotations;

namespace E_Voting_System.Models
{
    public class Party
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(10)]
        public string PartyCode { get; set; } = string.Empty;   // e.g. "ANC", "DA"

        public string? LogoUrl { get; set; }
        public string? ManifestoUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();
        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }
}
