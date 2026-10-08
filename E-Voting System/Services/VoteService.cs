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

        public async Task<bool> HasAlreadyVotedAsync(string userId, int electionId)
        {
            return await _context.VotingTokens
                .AnyAsync(t => t.UserId == userId && t.ElectionId == electionId && t.IsUsed);
        }

        public async Task<VotingToken?> IssueTokenAsync(string userId, int electionId)
        {
            if (await HasAlreadyVotedAsync(userId, electionId))
                return null;

            var existing = await _context.VotingTokens
                .FirstOrDefaultAsync(t => t.UserId == userId
                                       && t.ElectionId == electionId
                                       && !t.IsUsed);

            if (existing != null)
                return existing;

            var token = new VotingToken
            {
                UserId = userId,
                ElectionId = electionId,
                Token = GenerateSecureToken(),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            // Only set these if your VotingToken model has the properties
            // token.CreatedAt = DateTime.UtcNow;

            _context.VotingTokens.Add(token);
            await _context.SaveChangesAsync();
            return token;
        }

        public async Task<(bool Success, string Message)> CastVoteAsync(
            string userId,
            string tokenValue,
            int electionId,
            int nationalPartyId,
            int? provincialPartyId)
        {
            var token = await _context.VotingTokens
                .FirstOrDefaultAsync(t => t.Token == tokenValue
                                       && t.UserId == userId
                                       && t.ElectionId == electionId
                                       && !t.IsUsed);

            if (token == null)
                return (false, "Invalid or already used voting token.");

            // Create anonymized vote
            var vote = new Vote
            {
                ElectionId = electionId,
                PartyId = nationalPartyId,
                ProvincialPartyId = provincialPartyId,
                BallotType = provincialPartyId.HasValue ? "Both" : "National",
                CastAt = DateTime.UtcNow
            };

            token.IsUsed = true;
            token.UsedAt = DateTime.UtcNow;

            _context.Votes.Add(vote);

            _context.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                Action = "VoteCast",
                Timestamp = DateTime.UtcNow,
                Details = $"ElectionId={electionId}"
            });

            await _context.SaveChangesAsync();

            _logger.LogInformation("Vote cast successfully for user {UserId}", userId);

            return (true, "Your vote has been successfully recorded. Thank you for voting!");
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
