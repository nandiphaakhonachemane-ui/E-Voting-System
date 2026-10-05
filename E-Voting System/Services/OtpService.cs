using E_Voting_System.Data;
using E_Voting_System.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace E_Voting_System.Services
{
    public class OtpService : IOtpService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;          // We will create this
        private readonly ILogger<OtpService> _logger;

        public OtpService(
            ApplicationDbContext context,
            IEmailSender emailSender,
            ILogger<OtpService> logger)
        {
            _context = context;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task<string> GenerateAndSendOtpAsync(string userId, string email, string? phoneNumber = null)
        {
            // Invalidate any previous unused OTPs
            var oldOtps = await _context.OtpVerifications
                .Where(o => o.UserId == userId && !o.IsUsed && o.Purpose == "Voting")
                .ToListAsync();

            foreach (var old in oldOtps)
                old.IsUsed = true;

            // Generate 6-digit OTP
            string code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            var otp = new OtpVerification
            {
                UserId = userId,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),   // Valid for 10 minutes
                Purpose = "Voting",
                IsUsed = false
            };

            _context.OtpVerifications.Add(otp);
            await _context.SaveChangesAsync();

            // Send Email
            string subject = "Your Voting OTP - SA Digital Voting";
            string body = $@"
                <h2>Voting Verification Code</h2>
                <p>Your One-Time Password is:</p>
                <h1 style='letter-spacing: 8px;'>{code}</h1>
                <p>This code is valid for <strong>10 minutes</strong>.</p>
                <p>If you did not request this, please ignore this email.</p>";

            await _emailSender.SendEmailAsync(email, subject, body);

            // TODO: Add SMS sending here later (Twilio / Vonage / etc.)
            // if (!string.IsNullOrEmpty(phoneNumber)) { ... }

            _logger.LogInformation("OTP sent to user {UserId}", userId);

            return code; // Only for testing – remove in production
        }

        public async Task<bool> ValidateOtpAsync(string userId, string code)
        {
            var otp = await _context.OtpVerifications
                .Where(o => o.UserId == userId
                         && o.Code == code
                         && o.Purpose == "Voting"
                         && !o.IsUsed
                         && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otp == null)
                return false;

            otp.IsUsed = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
