using E_Voting_System.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace E_Voting_System.Services
{
    public interface IResultsService
    {
        Task<ElectionResultsViewModel> GetResultsAsync(int electionId);
        Task<List<SeatAllocationResult>> CalculateSeatAllocationAsync(int electionId, int totalSeats);
    }
}
