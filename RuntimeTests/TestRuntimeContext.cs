// TestRuntimeContext (testing harness)
using ExecGraph.Abstractions.Common;
using ExecGraph.Abstractions.Data;
using ExecGraph.Abstractions.Trace;
using ExecGraph.Runtime.Abstractions.Runtime;
using System.Collections.ObjectModel;


public class TestRuntimeContext : IRuntimeContext
{
    public NodeId NodeId { get; init; } = NodeId.New();
    public RunMode RunMode { get; init; } = RunMode.Development;

    public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

    public TimeSpan? DeadlineRemaining => null;
    public IServiceProvider Services { get; init; } = null!;
    public IReadOnlyDictionary<string, DataValue> Inputs { get; private set; } = new ReadOnlyDictionary<string, DataValue>(new Dictionary<string, DataValue>());
    public IReadOnlyDictionary<string, object?> Properties { get; } = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>());
    public List<TraceEvent> Traces { get; } = new();
    private readonly Dictionary<string, DataValue> _outputs = new();
    public Dictionary<string, DataValue> Outputs { get; } = new();

    public TestRuntimeContext(NodeId id) => NodeId = id;
    public ValueTask SetOutputAsync(string portName, DataValue value)
    {
        _outputs[portName] = value;
        return ValueTask.CompletedTask;
    }
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



    public ValueTask WriteOutputStreamAsync(string portName, IAsyncEnumerable<DataValue> stream, CancellationToken ct = default)
    {
        // 测试实现可以选择 materialize，或记录流对象
        var list = new List<DataValue>();
        var enumerator = stream.GetAsyncEnumerator(ct);
        // 注意：在测试环境避免长时间等待；此处给出简化做法
        while (enumerator.MoveNextAsync().AsTask().Result)
        {
            list.Add(enumerator.Current);
        }
        _outputs[portName] = new DataValue(list.AsReadOnly(), new DataTypeId("stream"));
        return ValueTask.CompletedTask;
    }



    // 新增接口实现：
    public async ValueTask CommitOutputsAsync(IReadOnlyDictionary<string, DataValue> outputs, CancellationToken cancellationToken = default)
    {
        if (outputs == null || outputs.Count == 0) return;
        foreach (var kv in outputs)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _outputs[kv.Key] = kv.Value;
            await Task.CompletedTask;
        }
    }
}
