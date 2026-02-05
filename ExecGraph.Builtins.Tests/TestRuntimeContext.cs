using ExecGraph.Contracts.Common;
using ExecGraph.Contracts.Runtime;
using ExecGraph.Contracts.Trace;
using System.Collections.Concurrent;

namespace ExecGraph.Builtins.Tests
{
    /// <summary>
    /// 测试用的简单 IRuntimeContext 实现：
    /// - 支持通过 Inputs 字典注入输入
    /// - SetOutput 会记录到 Outputs 字典
    /// - Traces 会收集 EmitTrace 的事件
    /// </summary>
    public class TestRuntimeContext : IRuntimeContext
    {
        public NodeId NodeId { get; }

        public RunMode RunMode { get; set; } = RunMode.Automatic;

        public IDictionary<string, object?> Inputs { get; } = new Dictionary<string, object?>();
        public IDictionary<string, object?> Outputs { get; } = new Dictionary<string, object?>();
        public ConcurrentBag<object> Traces { get; } = new ConcurrentBag<object>();

        public TestRuntimeContext(NodeId nodeId)
        {
            NodeId = nodeId;
        }

        public T GetInput<T>(string portName)
        {
            if (Inputs.TryGetValue(portName, out var v))
            {
                if (v is T t) return t;
                if (v == null) return default!;
                // try convert
                return (T)Convert.ChangeType(v, typeof(T));
            }
            return default!;
        }

        public void SetOutput<T>(string portName, T value)
        {
            Outputs[portName] = value!;
            // 如果你的 Trace 类型为 DataWriteTrace，请替换为相应的类型
            Traces.Add(new { Node = NodeId, Port = portName, Value = value });
        }

        public void EmitTrace(TraceEvent trace)
        {
            Traces.Add(trace);
        }

        // 如果你的 IRuntimeContext 有其它成员，请在这里补全
    }
}
