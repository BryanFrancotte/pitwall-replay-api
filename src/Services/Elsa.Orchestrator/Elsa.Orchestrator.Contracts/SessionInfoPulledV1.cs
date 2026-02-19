namespace Elsa.Orchestrator.Contracts
{
    public sealed record SessionInfoPulledV1(
        string SessionId,
        string CorrelationId,
        string PayloadJson,
        DateTimeOffset PulledAtUtc
    );
}
