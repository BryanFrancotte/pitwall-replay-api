using Elsa.Orchestrator.Domain;
using Microsoft.EntityFrameworkCore;

namespace Elsa.Orchestrator.Infrastructure
{
    public sealed class OrchestratorDbContext : DbContext
    {
        public OrchestratorDbContext(DbContextOptions<OrchestratorDbContext> options) : base(options){ }
        
        public DbSet<SessionPullLedgerEntry> SessionPullLedgerEntries => Set<SessionPullLedgerEntry>(); 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SessionPullLedgerEntry>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasIndex(x => x.SessionId).IsUnique();

                b.Property(x => x.SessionId).IsRequired();
                b.Property(x => x.CorrelationId).IsRequired();
                b.Property(x => x.Status).IsRequired();
                b.Property(x => x.CreatedAtUtc).IsRequired();
                b.Property(x => x.UpdatedAtUtc).IsRequired();
            });
        }
    }
}
