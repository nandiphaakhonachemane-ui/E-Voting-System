using E_Voting_System.Models;
using Microsoft.AspNetCore.Mvc;

namespace E_Voting_System.ViewModels
{
    public class ElectionResultsViewModel
    {
        public Election Election { get; set; } = null!;
        public List<PartyResult> NationalResults { get; set; } = new();
        public List<PartyResult> ProvincialResults { get; set; } = new();
        public int TotalNationalVotes { get; set; }
        public int TotalProvincialVotes { get; set; }
    }

    public class PartyResult
    {
        public int PartyId { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public string PartyCode { get; set; } = string.Empty;
        public int Votes { get; set; }
        public double Percentage => 0; // calculated in view
    }

    public class SeatAllocationResult
    {
        public int PartyId { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public string PartyCode { get; set; } = string.Empty;
        public int Votes { get; set; }
        public int Quota { get; set; }
        public int Seats { get; set; }
        public int Remainder { get; set; }
    }
}
