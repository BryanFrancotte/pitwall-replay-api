namespace Elsa.Orchestrator.Contracts
{
    public sealed record SessionPullRequestedV1(
        string SessionId,
        string CorrelationId,
        DateTimeOffset RequestedAtUtc
    );
}
