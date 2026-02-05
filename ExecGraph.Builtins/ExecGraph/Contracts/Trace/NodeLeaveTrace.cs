using ExecGraph.Abstractions.Common;
using ExecGraph.Abstractions.Trace;

namespace ExecGraph.Contracts.Trace
{
    internal record NodeLeaveTrace(NodeId NodeId) : TraceEvent;
}