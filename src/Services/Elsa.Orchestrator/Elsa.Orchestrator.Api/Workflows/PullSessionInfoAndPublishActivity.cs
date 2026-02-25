using Elsa.Orchestrator.Application.Ports;
using Elsa.Orchestrator.Contracts;
using Elsa.Orchestrator.Domain;
using Elsa.Workflows;
using MassTransit;

namespace Elsa.Orchestrator.Api.Workflows
{
    public sealed class PullSessionInfoAndPublishActivity : Activity
    {
        public string SessionId { get; }
        public string CorrelationId { get; }

        public PullSessionInfoAndPublishActivity(string sessionId, string correlationId)
        {
            SessionId = sessionId;
            CorrelationId = correlationId;
        }

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var repo = context.GetRequiredService<ISessionPullLedgerRepository>();
            var source = context.GetRequiredService<IF1SessionSource>();
            var publish = context.GetRequiredService<IPublishEndpoint>();

            var entry = await repo.FindBySessionIdAsync(SessionId, context.CancellationToken)
                ?? new Domain.SessionPullLedgerEntry { SessionId = SessionId, CorrelationId = CorrelationId};

            if (entry.Status == Domain.SessionPullStatus.Completed)
            {
                await context.CompleteActivityAsync();
                return;
            }

            entry.Status = SessionPullStatus.InProgress;
            await repo.UpdateAsync(entry, context.CancellationToken);

            try
            {
                var json = await source.GetSessionInfoJsonAsync(SessionId, context.CancellationToken);
                var evt = new SessionInfoPulledV1(SessionId, CorrelationId, json, DateTimeOffset.UtcNow);

                await publish.Publish(evt, pubCtx =>
                {
                    pubCtx.SetRoutingKey(RabbitMqTopology.Routing.SessionPullRequestedV1);
                }, context.CancellationToken);

                entry.Status = SessionPullStatus.Completed;
                entry.CompletedAtUtc = DateTimeOffset.UtcNow;
                entry.Error = null;
                await repo.UpdateAsync(entry, context.CancellationToken);
            }
            catch (Exception ex) {
                entry.Status = SessionPullStatus.Failed;
                entry.Error = ex.ToString();
                await repo.UpdateAsync(entry, context.CancellationToken);
                throw;
            }

            await context.CompleteActivityAsync();
        }
    }
}
