using Microsoft.AspNetCore.Mvc;

namespace E_Voting_System.Models
{
    public class AuditLog
    {
        public long Id { get; set; }
        public string? UserId { get; set; }
        public string Action { get; set; } = string.Empty;   // Login, Voted, AdminVerified, etc.
        public string? IpAddress { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Details { get; set; }                 // Never store the actual vote choice
    }
}
