using E_Voting_System.Data;
using E_Voting_System.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Voting_System.Services
{
    public class ResultsService : IResultsService
    {
        private readonly ApplicationDbContext _context;

        public ResultsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ElectionResultsViewModel> GetResultsAsync(int electionId)
        {
            var election = await _context.Elections.FindAsync(electionId)
                ?? throw new Exception("Election not found");

            // National votes
            var nationalVotes = await _context.Votes
                .Where(v => v.ElectionId == electionId)
                .GroupBy(v => v.PartyId)
                .Select(g => new PartyResult
                {
                    PartyId = g.Key,
                    PartyName = g.First().Party.Name,
                    PartyCode = g.First().Party.PartyCode,
                    Votes = g.Count()
                })
                .OrderByDescending(r => r.Votes)
                .ToListAsync();

            // Provincial votes (if any)
            var provincialVotes = await _context.Votes
                .Where(v => v.ElectionId == electionId && v.ProvincialPartyId != null)
                .GroupBy(v => v.ProvincialPartyId)
                .Select(g => new PartyResult
                {
                    PartyId = g.Key!.Value,
                    PartyName = g.First().ProvincialParty!.Name,
                    PartyCode = g.First().ProvincialParty!.PartyCode,
                    Votes = g.Count()
                })
                .OrderByDescending(r => r.Votes)
                .ToListAsync();

            int totalNationalVotes = nationalVotes.Sum(v => v.Votes);
            int totalProvincialVotes = provincialVotes.Sum(v => v.Votes);

            return new ElectionResultsViewModel
            {
                Election = election,
                NationalResults = nationalVotes,
                ProvincialResults = provincialVotes,
                TotalNationalVotes = totalNationalVotes,
                TotalProvincialVotes = totalProvincialVotes
            };
        }

        public async Task<List<SeatAllocationResult>> CalculateSeatAllocationAsync(int electionId, int totalSeats)
        {
            var results = await GetResultsAsync(electionId);
            var partyVotes = results.NationalResults;

            if (!partyVotes.Any() || totalSeats <= 0)
                return new List<SeatAllocationResult>();

            // ===== DROOP QUOTA =====
            // Quota = (Total Votes / (Seats + 1)) + 1
            int totalVotes = results.TotalNationalVotes;
            int quota = (totalVotes / (totalSeats + 1)) + 1;

            var allocation = partyVotes.Select(p => new SeatAllocationResult
            {
                PartyId = p.PartyId,
                PartyName = p.PartyName,
                PartyCode = p.PartyCode,
                Votes = p.Votes,
                Quota = quota,
                Seats = p.Votes / quota,               // Initial seats
                Remainder = p.Votes % quota            // Remainder for surplus
            }).ToList();

            int seatsAllocated = allocation.Sum(a => a.Seats);
            int remainingSeats = totalSeats - seatsAllocated;

            // Allocate remaining seats by largest remainder
            var orderedByRemainder = allocation
                .OrderByDescending(a => a.Remainder)
                .ThenByDescending(a => a.Votes)
                .ToList();

            for (int i = 0; i < remainingSeats && i < orderedByRemainder.Count; i++)
            {
                orderedByRemainder[i].Seats += 1;
            }

            return allocation.OrderByDescending(a => a.Seats)
                             .ThenByDescending(a => a.Votes)
                             .ToList();
        }
    }
}
