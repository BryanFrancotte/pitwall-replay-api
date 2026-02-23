namespace Elsa.Orchestrator.Application.Ports
{
    public interface IF1SessionSource
    {
        Task<string> GetSessionInfoJsonAsync (string sessionId, CancellationToken cancellationToken);
    }
}
