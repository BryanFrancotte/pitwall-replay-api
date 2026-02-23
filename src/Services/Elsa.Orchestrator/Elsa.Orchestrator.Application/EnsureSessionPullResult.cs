using Elsa.Orchestrator.Domain;

namespace Elsa.Orchestrator.Application
{
    public sealed record EnsureSessionPullResult(
        string SessionId,
        string CorrelationId,
        SessionPullStatus PullStatus
    );
}
