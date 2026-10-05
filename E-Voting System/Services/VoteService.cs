using E_Voting_System.Data;
using E_Voting_System.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace E_Voting_System.Services
{
    public class VoteService : IVoteService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VoteService> _logger;

        public VoteService(ApplicationDbContext context, ILogger<VoteService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> IsElectionOpenAsync(int electionId)
        {
            var election = await _context.Elections.FindAsync(electionId);
            if (election == null) return false;

            var now = DateTime.UtcNow;
            return election.IsOpen && now >= election.StartDate && now <= election.EndDate;
        }

        public async Task<bool> HasAlreadyVotedAsync(string userId, int electionId)
        {
            return await _context.VotingTokens
                .AnyAsync(t => t.UserId == userId
                            && t.ElectionId == electionId
                            && t.IsUsed);
        }

        public async Task<VotingToken?> IssueTokenAsync(string userId, int electionId)
        {
            // Prevent issuing if already voted
            if (await HasAlreadyVotedAsync(userId, electionId))
                return null;

            // Check if a valid unused token already exists
            var existing = await _context.VotingTokens
                .FirstOrDefaultAsync(t => t.UserId == userId
                                       && t.ElectionId == electionId
                                       && !t.IsUsed);

            if (existing != null)
                return existing;

            // Create new one-time token
            var token = new VotingToken
            {
                UserId = userId,
                ElectionId = electionId,
                Token = GenerateSecureToken(),
                IssuedAt = DateTime.UtcNow,
                IsUsed = false
            };

            _context.VotingTokens.Add(token);
            await _context.SaveChangesAsync();

            return token;
        }

        public async Task<(bool Success, string Message)> CastVoteAsync(
            string userId,
            string tokenValue,
            int electionId,
            int nationalPartyId,
            int? provincialPartyId = null)
        {
            // 1. Election must be open
            if (!await IsElectionOpenAsync(electionId))
                return (false, "Election is closed or not yet open.");

            // 2. Find the token
            var token = await _context.VotingTokens
                .FirstOrDefaultAsync(t => t.Token == tokenValue
                                       && t.UserId == userId
                                       && t.ElectionId == electionId);

            if (token == null)
                return (false, "Invalid voting token.");

            if (token.IsUsed)
                return (false, "This token has already been used.");

            // 3. Double-check user hasn't voted (race-condition protection)
            if (await HasAlreadyVotedAsync(userId, electionId))
                return (false, "You have already cast your vote.");

            // 4. Validate parties exist
            var nationalParty = await _context.Parties.FindAsync(nationalPartyId);
            if (nationalParty == null || !nationalParty.IsActive)
                return (false, "Invalid national party selection.");

            if (provincialPartyId.HasValue)
            {
                var provincialParty = await _context.Parties.FindAsync(provincialPartyId.Value);
                if (provincialParty == null || !provincialParty.IsActive)
                    return (false, "Invalid provincial party selection.");
            }

            // 5. Create anonymised vote (NO UserId stored here)
            var vote = new Vote
            {
                ElectionId = electionId,
                PartyId = nationalPartyId,
                ProvincialPartyId = provincialPartyId,
                BallotType = provincialPartyId.HasValue ? "Both" : "National",
                CastAt = DateTime.UtcNow
            };

            // 6. Mark token as used
            token.IsUsed = true;
            token.UsedAt = DateTime.UtcNow;

            // 7. Write audit log (only that the user voted – never what they voted)
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                Action = "VoteCast",
                Timestamp = DateTime.UtcNow,
                Details = $"ElectionId={electionId}"
            });

            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Vote successfully cast for user {UserId} in election {ElectionId}", userId, electionId);

            return (true, "Your vote has been successfully recorded. Thank you for voting.");
        }

        private static string GenerateSecureToken()
        {
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes)
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "")
                .Substring(0, 40);
        }
    }
}
