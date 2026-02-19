namespace Elsa.Orchestrator.Domain
{
    public sealed class SessionPullLedgerEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string SessionId { get; set; } = default!;
        public string CorrelationId { get; set; } = default!;

        public SessionPullStatus Status { get; set; } = SessionPullStatus.Pending;
        public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? CompletedAtUtc { get; set; }
        public string? Error {  get; set; }
    }
}