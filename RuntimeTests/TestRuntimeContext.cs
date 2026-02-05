// TestRuntimeContext (testing harness)
using ExecGraph.Contracts.Common;
using ExecGraph.Contracts.Data;
using ExecGraph.Contracts.Runtime;
using ExecGraph.Contracts.Trace;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

public class TestRuntimeContext : IRuntimeContext
{
    public NodeId NodeId { get; }
    public RunMode RunMode { get; set; } = RunMode.Automatic;

    public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

    public TimeSpan? DeadlineRemaining { get; }
    public IServiceProvider Services { get; } = null!;
    public IReadOnlyDictionary<string, DataValue> Inputs { get; private set; } = new ReadOnlyDictionary<string, DataValue>(new Dictionary<string, DataValue>());
    public IReadOnlyDictionary<string, object?> Properties { get; } = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>());
    public List<TraceEvent> Traces { get; } = new();

    public Dictionary<string, DataValue> Outputs { get; } = new();

    public TestRuntimeContext(NodeId id) => NodeId = id;

    public void SetInputs(params (string name, object? value)[] pairs)
    {
        var dict = new Dictionary<string, DataValue>();
        foreach (var p in pairs)
        {
            var dv = new DataValue(p.value, new DataTypeId(p.value?.GetType().Name ?? "any"));
            dict[p.name] = dv;
        }
        Inputs = new ReadOnlyDictionary<string, DataValue>(dict);
    }

    public T? GetInput<T>(string name)
    {
        if (Inputs.TryGetValue(name, out var dv) && dv.Value is T t) return t;
        return default;
    }

    public IReadOnlyDictionary<string, DataValue> GetAllInputs() => Inputs;

    public void EmitTrace(TraceEvent ev) { Traces.Add(ev); }

    public async ValueTask SetOutputAsync(string portName, DataValue value)
    {
        Outputs[portName] = value;
        await Task.CompletedTask;
    }

    public ValueTask WriteOutputStreamAsync(string portName, IAsyncEnumerable<DataValue> stream, CancellationToken ct = default)
    {
        // testing helper: collect stream to Outputs as array
        _ = Task.Run(async () =>
        {
            var list = new List<DataValue>();
            await foreach (var item in stream.WithCancellation(ct))
            {
                list.Add(item);
            }
            Outputs[portName] = new DataValue(list.ToArray(), new DataTypeId("array"));
        }, ct);
        return ValueTask.CompletedTask;
    }
}
