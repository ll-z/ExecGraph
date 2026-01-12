using ExecGraph.Contracts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Contracts.Trace
{

    public sealed record NodeEnterTrace : TraceEvent;
    public sealed record NodeLeaveTrace : TraceEvent;

    public sealed record DataWriteTrace : TraceEvent
    {
        public string Port { get; init; } = string.Empty;
        public object? Value { get; init; }
    }
}
