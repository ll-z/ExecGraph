using ExecGraph.Contracts.Common;
using ExecGraph.Contracts.Data;
using ExecGraph.Contracts.Graph;
using ExecGraph.Contracts.Runtime;
using ExecGraph.Contracts.Trace;
using ExecGraph.Runtime; // 如果 RuntimeContext 在此命名空间
using ExecGraph.Runtime.Trace;
using ExecGraph.Runtime.VM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

// 注意：该实现假设存在 DataValue, DataWriteTrace 等类型与你现有实现一致。
// 如果你的 DataStore 提供异步 API（例如 SetOutputAsync），可以在 SetOutputAsync 中直接调用。

public sealed class RuntimeContext : IRuntimeContext
{
    private readonly NodeId _nodeId;
    private readonly DataStore _store;
    private readonly TraceEmitter _trace;
    private readonly RunMode _runMode;

    // 新增可选依赖 / 运行时能力
    private readonly CancellationToken _cancellationToken;
    private readonly TimeSpan? _deadlineRemaining;
    private readonly IServiceProvider? _services;
    private readonly IReadOnlyDictionary<string, DataValue> _inputs;
    private readonly IReadOnlyDictionary<string, object?> _properties;

    public NodeId NodeId => _nodeId;
    public RunMode RunMode => _runMode;
    public CancellationToken CancellationToken => _cancellationToken;
    public TimeSpan? DeadlineRemaining => _deadlineRemaining;
    public IServiceProvider? Services => _services;

    /// <summary>
    /// Inputs: 快照（只读）——节点应从这里读取输入而不是直接问 DataStore（提高并发安全性）。
    /// </summary>
    public IReadOnlyDictionary<string, DataValue> Inputs => _inputs;

    /// <summary>
    /// 运行时/执行相关的自定义属性（例如 ExecutionId / Attempt / etc.）
    /// </summary>
    public IReadOnlyDictionary<string, object?> Properties => _properties;

    /// <summary>
    /// 构造函数（内部）：最好由 RuntimeHost/ExecutionEngine 在调度节点时构造并传入 inputs 快照与 cancellation token。
    /// </summary>
    internal RuntimeContext(
        NodeId nodeId,
        DataStore store,
        TraceEmitter trace,
        RunMode runMode,
        IReadOnlyDictionary<string, DataValue>? inputs = null,
        IReadOnlyDictionary<string, object?>? properties = null,
        IServiceProvider? services = null,
        CancellationToken cancellationToken = default,
        TimeSpan? timeSpan = default)
    {
        _nodeId = nodeId;
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _trace = trace ?? throw new ArgumentNullException(nameof(trace));
        _runMode = runMode;
        _cancellationToken = cancellationToken;
        _deadlineRemaining = timeSpan;
        _services = services;

        // 保证 inputs 不为 null（使用空只读字典作为默认）
        _inputs = inputs ?? new ReadOnlyDictionary<string, DataValue>(new Dictionary<string, DataValue>());
        _properties = properties ?? new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>());
    }

    // 兼容原来的同步读（优先从 Inputs 快照读，找不到再回退到 DataStore）
    public T GetInput<T>(string portName)
    {
        if (portName == null) throw new ArgumentNullException(nameof(portName));

        // 优先从传入的快照中读取（更安全、并发友好）
        if (_inputs.TryGetValue(portName, out var dv))
        {
            if (dv.Value == null) return default!;
            if (dv.Value is T t) return t;
            try
            {
                return (T)Convert.ChangeType(dv.Value, typeof(T));
            }
            catch
            {
                // 类型转换失败时返回默认
                return default!;
            }
        }

        // 如果快照中没有，再退回旧的 DataStore 获取（兼容路径）
        try
        {
            return _store.GetInput<T>(_nodeId, portName);
        }
        catch
        {
            return default!;
        }
    }

    /// <summary>
    /// 旧的同步写法（保留），仍然发出 DataWriteTrace
    /// </summary>
    public void SetOutput<T>(string portName, DataValue value)
    {
        if (portName == null) throw new ArgumentNullException(nameof(portName));
        if (value == null) throw new ArgumentNullException(nameof(value));

        // 同步写入 DataStore（原来的行为）
        _store.SetOutput(_nodeId, portName, value);

        // 发 DataWriteTrace
        _trace.Emit(new DataWriteTrace
        {
            NodeId = _nodeId,
            Port = portName,
            Value = value
        });
    }

    /// <summary>
    /// 异步写输出（供异步节点调用）。
    /// 如果底层 DataStore 有异步 API，可在此直接调用；否则使用同步实现并返回已完成的 ValueTask。
    /// </summary>
    public ValueTask SetOutputAsync(string portName, DataValue value)
    {
        // 如果你提供了 IAsyncDataStore，可以做类型判断并调用异步方法：
        // if (_store is IAsyncDataStore asyncStore) return asyncStore.SetOutputAsync(_nodeId, portName, value);
        // 否则回退到同步实现（不会阻塞因为这是本地操作），并返回已完成的任务以便节点 await。
        SetOutput<object>(portName, value); // 使用上面的同步实现（泛型 T 无关）
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 将流式输出写到端口上（逐项写入）。用于长流输出场景。
    /// </summary>
    public async ValueTask WriteOutputStreamAsync(string portName, IAsyncEnumerable<DataValue> stream, CancellationToken ct = default)
    {
        if (portName == null) throw new ArgumentNullException(nameof(portName));
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        var linked = CancellationToken.None;
        try
        {
            linked = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken, ct).Token;
        }
        catch
        {
            // 如果创建失败（极少见），仍然使用 ct
            linked = ct;
        }

        await foreach (var dv in stream.WithCancellation(linked).ConfigureAwait(false))
        {
            // 这里逐项写入 DataStore；如果后续有优化可批量/流式路由
            _store.SetOutput(_nodeId, portName, dv);
            _trace.Emit(new DataWriteTrace
            {
                NodeId = _nodeId,
                Port = portName,
                Value = dv
            });
        }
    }

    /// <summary>
    /// 返回当前 Inputs 快照（帮助节点一次性读取所有输入）
    /// </summary>
    public IReadOnlyDictionary<string, DataValue> GetAllInputs() => Inputs;

    /// <summary>
    /// 发 trace（原样转发给 TraceEmitter）
    /// </summary>
    public void EmitTrace(TraceEvent trace)
    {
        if (trace == null) throw new ArgumentNullException(nameof(trace));
        _trace.Emit(trace);
    }
}
