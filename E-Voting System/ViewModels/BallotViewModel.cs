using E_Voting_System.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace E_Voting_System.ViewModels
{
    public class BallotViewModel
    {
        public int ElectionId { get; set; }
        public Election? Election { get; set; }
        public string Token { get; set; } = string.Empty;

        public List<Party> Parties { get; set; } = new();

        [Required(ErrorMessage = "Please select a national party")]
        [Display(Name = "National Party")]
        public int SelectedNationalPartyId { get; set; }

        [Display(Name = "Provincial Party (Optional)")]
        public int? SelectedProvincialPartyId { get; set; }
    }
}