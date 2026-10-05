using E_Voting_System.Models;
using Microsoft.AspNetCore.Mvc;

namespace E_Voting_System.Services
{
    public interface IVoteService
    {
        Task<VotingToken?> IssueTokenAsync(string userId, int electionId);
        Task<bool> HasAlreadyVotedAsync(string userId, int electionId);
        Task<(bool Success, string Message)> CastVoteAsync(
            string userId,
            string token,
            int electionId,
            int nationalPartyId,
            int? provincialPartyId = null);
        Task<bool> IsElectionOpenAsync(int electionId);
    }
}
