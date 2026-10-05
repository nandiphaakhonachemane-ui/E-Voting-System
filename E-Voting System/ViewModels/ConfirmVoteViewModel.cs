using E_Voting_System.Models;
using Microsoft.AspNetCore.Mvc;

namespace E_Voting_System.ViewModels
{
    public class ConfirmVoteViewModel
    {
        public int ElectionId { get; set; }
        public string Token { get; set; } = string.Empty;

        public int SelectedNationalPartyId { get; set; }
        public int? SelectedProvincialPartyId { get; set; }

        public Party NationalParty { get; set; } = null!;
        public Party? ProvincialParty { get; set; }
    }
}
