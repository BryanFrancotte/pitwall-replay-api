using Elsa.Orchestrator.Domain;

namespace Elsa.Orchestrator.Application.Ports
{
    public interface ISessionPullLedgerRepository
    {
        Task<SessionPullLedgerEntry?> FindBySessionIdAsync(string sessionId, CancellationToken cancellationToken);
        Task AddAsync (SessionPullLedgerEntry entry, CancellationToken cancellationToken);
        Task UpdateAsync (SessionPullLedgerEntry entry, CancellationToken cancellationToken);
    }
}
