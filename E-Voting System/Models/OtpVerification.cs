using Microsoft.AspNetCore.Mvc;

namespace E_Voting_System.Models
{
    public class OtpVerification
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;          // 6-digit OTP
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public string Purpose { get; set; } = "Voting";           // Voting / Login etc.
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
