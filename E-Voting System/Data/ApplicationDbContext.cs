using E_Voting_System.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace E_Voting_System.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Party> Parties { get; set; }
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<Election> Elections { get; set; }
        public DbSet<VotingToken> VotingTokens { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<OtpVerification> OtpVerifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Unique SA ID
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.SouthAfricanId)
                .IsUnique();

            // Unique party code
            builder.Entity<Party>()
                .HasIndex(p => p.PartyCode)
                .IsUnique();

            // One token per user per election
            builder.Entity<VotingToken>()
                .HasIndex(t => new { t.UserId, t.ElectionId })
                .IsUnique();

            // Vote has no navigation to User – deliberate
            builder.Entity<Vote>()
                .HasOne(v => v.Party)
                .WithMany(p => p.Votes)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
