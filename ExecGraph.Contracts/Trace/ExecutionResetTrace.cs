using ExecGraph.Abstractions.Common;
using ExecGraph.Abstractions.Trace;

namespace ExecGraph.Contracts.Trace
{
    /// <summary>
    /// Indicates a manually confirmed execution restart.
    /// This marks an execution epoch boundary.
    /// </summary>
    /// <summary>
    /// Indicates a manually confirmed execution restart.
    /// This marks an execution epoch boundary.
    /// </summary>
    public sealed record ExecutionResetTrace(
        long EpochFrom,
        long EpochTo,
        NodeId? StartNode
    ) : TraceEvent;
}
