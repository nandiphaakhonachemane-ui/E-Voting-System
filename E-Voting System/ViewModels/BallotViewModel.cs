using E_Voting_System.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace E_Voting_System.ViewModels
{
    public class BallotViewModel
    {
        public Election Election { get; set; } = null!;
        public List<Party> Parties { get; set; } = new();
        public string Token { get; set; } = string.Empty;

        public int ElectionId => Election.Id;

        [Required(ErrorMessage = "Please select a party for the National Ballot")]
        public int SelectedNationalPartyId { get; set; }

        public int? SelectedProvincialPartyId { get; set; }
    }
}
