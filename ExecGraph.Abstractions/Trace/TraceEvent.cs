
using ExecGraph.Abstractions.Common;
using ExecGraph.Abstractions.Data;

namespace ExecGraph.Abstractions.Trace
{
    public abstract record TraceEvent
    {
        public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
        public NodeId NodeId { get; init; }
        public IReadOnlyDictionary<string, object?>? Meta { get; init; }
    }

    public sealed record NodeEnterTrace : TraceEvent { public NodeEnterTrace() { } }

    public sealed record NodeLeaveTrace : TraceEvent
    {
        public TimeSpan? Duration { get; init; }
    }

    // <--- 确保存在这个类型 ---->
    public sealed record NodeErrorTrace : TraceEvent
    {
        public string? ErrorMessage { get; init; }
        public string? StackTrace { get; init; }
    }
    // -----------------------------

    public sealed record IOTrace : TraceEvent
    {
        public IReadOnlyDictionary<string, DataValue>? Inputs { get; init; }
        public IReadOnlyDictionary<string, DataValue>? Outputs { get; init; }
    }
}
