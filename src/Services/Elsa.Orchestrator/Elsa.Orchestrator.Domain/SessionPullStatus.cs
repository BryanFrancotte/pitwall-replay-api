using System;
using System.Collections.Generic;
using System.Text;

namespace Elsa.Orchestrator.Domain
{
    public enum SessionPullStatus
    {
        Pending = 0,
        InProgress = 1,
        Completed = 2,
        Failed = 3
    }
}
