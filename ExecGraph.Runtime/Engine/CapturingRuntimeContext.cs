// ExecGraph.Runtime/Engine/CapturingRuntimeContext.cs
using ExecGraph.Abstractions.Common;
using ExecGraph.Abstractions.Data;
using ExecGraph.Abstractions.Trace;

using ExecGraph.Runtime.Abstractions.Runtime;


namespace ExecGraph.Runtime.Engine
{
    /// <summary>
    /// 一个包装 IRuntimeContext 的捕获器（用于包装旧节点的执行）。
    /// 它会拦截 SetOutputAsync/WriteOutputStreamAsync/EmitTrace 调用，
    /// 把输出和 trace 缓存到本地集合，供 adapter 在执行完成后构造 ExecutionResult。
    /// </summary>
    internal sealed class CapturingRuntimeContext : IRuntimeContext
    {
        private readonly IRuntimeContext _inner;

        public CapturingRuntimeContext(IRuntimeContext inner)
        {
            _inner = inner;
            CapturedOutputs = new Dictionary<string, DataValue>();
            CapturedTraces = new List<TraceEvent>();
        }

        public Dictionary<string, DataValue> CapturedOutputs { get; }
        public List<TraceEvent> CapturedTraces { get; }

        public NodeId NodeId => _inner.NodeId;
        public RunMode RunMode => _inner.RunMode;
        public CancellationToken CancellationToken => _inner.CancellationToken;
        public TimeSpan? DeadlineRemaining => _inner.DeadlineRemaining;
        public IServiceProvider Services => _inner.Services;
        public IReadOnlyDictionary<string, DataValue> Inputs => _inner.Inputs;
        public IReadOnlyDictionary<string, object?> Properties => _inner.Properties;

        public T? GetInput<T>(string name) => _inner.GetInput<T>(name);

        // Intercept SetOutputAsync: buffer instead of forwarding to inner context.
        public ValueTask SetOutputAsync(string portName, DataValue value)
        {
            CapturedOutputs[portName] = value;
            return ValueTask.CompletedTask;
        }

        // For stream, we either buffer an indicator or materialize; simplest: materialize into list.
        public async ValueTask WriteOutputStreamAsync(string portName, IAsyncEnumerable<DataValue> stream, CancellationToken ct = default)
        {
            var list = new List<DataValue>();
            await foreach (var dv in stream.WithCancellation(ct))
            {
                list.Add(dv);
            }
            // store as a DataValue that wraps an IReadOnlyList<DataValue> and a special DataTypeId
            CapturedOutputs[portName] = new DataValue(list.AsReadOnly(), new DataTypeId("stream"));
        }

        public void EmitTrace(TraceEvent trace)
        {
            CapturedTraces.Add(trace);
        }

        // In case someone calls CommitOutputsAsync on the capturing context, forward to inner.
        public ValueTask CommitOutputsAsync(IReadOnlyDictionary<string, DataValue> outputs, CancellationToken cancellationToken = default)
            => _inner.CommitOutputsAsync(outputs, cancellationToken);
    }
}
