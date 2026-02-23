using Elsa.Orchestrator.Application.Ports;
using Elsa.Orchestrator.Domain;
using Microsoft.EntityFrameworkCore;

namespace Elsa.Orchestrator.Infrastructure
{
    public sealed class SessionPullLedgerRepository : ISessionPullLedgerRepository
    {
        private readonly OrchestratorDbContext _dbContext;

        public SessionPullLedgerRepository(OrchestratorDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(SessionPullLedgerEntry entry, CancellationToken cancellationToken)
        {
            _dbContext.SessionPullLedgerEntries.Add(entry);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task<SessionPullLedgerEntry?> FindBySessionIdAsync(string sessionId, CancellationToken cancellationToken) => 
            _dbContext.SessionPullLedgerEntries.AsNoTracking().SingleOrDefaultAsync(x => x.SessionId == sessionId, cancellationToken);

        public async Task UpdateAsync(SessionPullLedgerEntry entry, CancellationToken cancellationToken)
        {
            entry.UpdatedAtUtc = DateTimeOffset.UtcNow;
            _dbContext.SessionPullLedgerEntries.Update(entry);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
