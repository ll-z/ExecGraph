

using ExecGraph.Abstractions.Common;
using ExecGraph.Abstractions.Data;
using ExecGraph.Abstractions.Trace;

namespace ExecGraph.Runtime.Abstractions.Runtime
{

    // 用于标注运行时节点的特性（示例）
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public sealed class RuntimeNodeAttribute : System.Attribute
    {
        public string Name { get; }
        public string? Description { get; set; }
        public RuntimeNodeAttribute(string name) => Name = name;
    }
    // 简化版 IRuntimeContext：增加了 GetAllInputs 方法（不用考虑兼容性）
    public interface IRuntimeContext
    {
       
        NodeId NodeId { get; }
        RunMode RunMode { get; }

        // Cancellation / Deadline
        CancellationToken CancellationToken { get; }
        TimeSpan? DeadlineRemaining { get; }

        // 服务注入
        IServiceProvider Services { get; }

        // 输入（只读字典，保证按插入顺序或按 NodeModel 定义的顺序）
        IReadOnlyDictionary<string, DataValue> Inputs { get; }

        // 便捷方法：按名获取输入（若不存在返回 default）
        T? GetInput<T>(string name);

        //void SetOutput<T>(string portName, T value);

        // 批量输出（一个端口写入单个 DataValue）
        ValueTask SetOutputAsync(string portName, DataValue value);

        // 流式输出（可用于逐条发送、长流）
        ValueTask WriteOutputStreamAsync(string portName, IAsyncEnumerable<DataValue> stream, CancellationToken ct = default);

        // interface IRuntimeContext 里新增：
        /// <summary>
        /// 将节点的 outputs 原子地提交到 runtime（runtime 负责校验 port/类型、触发后续节点等）。
        /// </summary>
        ValueTask CommitOutputsAsync(IReadOnlyDictionary<string, DataValue> outputs, CancellationToken cancellationToken = default);

        void EmitTrace(TraceEvent trace);
        // 运行时可扩展属性（键-值）
        IReadOnlyDictionary<string, object?> Properties { get; }


    }

 }
