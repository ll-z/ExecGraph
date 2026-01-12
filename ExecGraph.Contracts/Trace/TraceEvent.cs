using ExecGraph.Contracts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Contracts.Trace
{
    public abstract record TraceEvent
    {
        public NodeId NodeId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    }

}
