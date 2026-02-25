using Elsa.Orchestrator.Api.Workflows;
using Elsa.Orchestrator.Application.Ports;
using Elsa.Orchestrator.Contracts;
using Elsa.Orchestrator.Domain;
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using MassTransit;

namespace Elsa.Orchestrator.Api.Consumers
{
    public sealed class SessionPullRequestedConsumer : IConsumer<SessionPullRequestedV1>
    {
        private readonly ISessionPullLedgerRepository _repo;
        private readonly IWorkflowRunner _runner;

        public SessionPullRequestedConsumer(ISessionPullLedgerRepository repo, IWorkflowRunner runner)
        {
            _repo = repo;
            _runner = runner;
        }

        public async Task Consume(ConsumeContext<SessionPullRequestedV1> context)
        {
            var msg = context.Message;

            var existing = await _repo.FindBySessionIdAsync(msg.SessionId, context.CancellationToken);
            if (existing?.Status == SessionPullStatus.Completed)
                return;

            var workflow = new Sequence
            {
                Activities =
                {
                    new WriteLine($"Pulling SessionInfo for session={msg.SessionId}"),
                    new PullSessionInfoAndPublishActivity(msg.SessionId, msg.CorrelationId),
                    new WriteLine($"Done for session={msg.SessionId}"),
                }
            };

            await _runner.RunAsync(activity: workflow, cancellationToken: context.CancellationToken);
        }
    }
}
