using Elsa.Orchestrator.Application.Ports;
using Elsa.Orchestrator.Contracts;
using Elsa.Orchestrator.Domain;
using MassTransit;
using MassTransit.Internals.Caching;

namespace Elsa.Orchestrator.Application
{
    public sealed class EnsureSessionPullService
    {
        private readonly ISessionPullLedgerRepository _repo;
        private readonly IPublishEndpoint _publish;

        public EnsureSessionPullService(ISessionPullLedgerRepository repo, IPublishEndpoint publish)
        {
            _repo = repo;
            _publish = publish;
        }

        public async Task<EnsureSessionPullResult> EnsureSessionPullAsync(string sessionId, CancellationToken cancellationToken)
        {
            var existing = await _repo.FindBySessionIdAsync(sessionId, cancellationToken);

            if (existing is not null)
                return new EnsureSessionPullResult(existing.SessionId, existing.CorrelationId, existing.Status);

            var correlationId = NewId.NextGuid().ToString("N");
            var entry = new SessionPullLedgerEntry
            {
                SessionId = sessionId,
                CorrelationId = correlationId,
                Status = SessionPullStatus.Pending
            };

            await _repo.AddAsync(entry, cancellationToken);

            var cmd = new SessionPullRequestedV1(sessionId, correlationId, DateTimeOffset.UtcNow);
            await _publish.Publish(cmd, context => 
            {
                context.SetRoutingKey(RabbitMqTopology.Routing.SessionPullRequestedV1);
            }, cancellationToken);

            return new EnsureSessionPullResult(sessionId, correlationId, SessionPullStatus.Pending);
        }
    }
}
