namespace Elsa.Orchestrator.Contracts
{
    public static class RabbitMqTopology
    {
        public const string CommandsExchange = "pitwall.cmd";
        public const string EventsExchange = "pitwall.evt";

        public static class Routing
        {
            public const string SessionPullRequestedV1 = "f1.session.pull.requested.v1";
            public const string SessionPulledV1 = "f1.sessioninfo.pulled.v1";
        }

        public const string ElsaSessionPullQueue = "elsa.sessionpull";
    }
}
