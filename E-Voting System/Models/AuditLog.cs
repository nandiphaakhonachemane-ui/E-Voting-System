using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace E_Voting_System.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [StringLength(450)]
        public string? UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Details { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
