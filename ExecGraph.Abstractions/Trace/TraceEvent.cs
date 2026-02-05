
using ExecGraph.Abstractions.Common;

namespace ExecGraph.Abstractions.Trace
{
    public abstract record TraceEvent
    {
        public NodeId NodeId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    }

     public sealed record NodeEnterTrace : TraceEvent
    {
        public NodeId NodeId { get; init; }
    }

    public sealed record NodeLeaveTrace : TraceEvent
    {
        public NodeId NodeId { get; init; }
    }

    public sealed record DataWriteTrace : TraceEvent
    {
        public string Port { get; init; } = string.Empty;
        public object? Value { get; init; }
    }

}
