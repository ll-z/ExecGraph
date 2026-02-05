
using ExecGraph.Abstractions.Common;
using ExecGraph.Abstractions.Data;
using ExecGraph.Abstractions.Trace;
using ExecGraph.Contracts.Graph;
using ExecGraph.Runtime.Abstractions.Runtime;


namespace ExecGraph.Builtins.Nodes
{
    [RuntimeNode("Constant", Description = "Emit a constant value (configured via constructor or NodeModel)")]
    public sealed class ConstantNode : IRuntimeNode
    {
        public NodeId Id { get; }
        private readonly DataValue? _value;

        // 用于直接测试/手动实例化
        public ConstantNode(NodeId id, DataValue? value)
        {
            Id = id;
            _value = value;
        }

        // 用于运行时通过 NodeModel 创建（如果你的 runtime 以 NodeModel 为参数）
        public ConstantNode(NodeId id, NodeModel model)
        {
            Id = id;
            // 假设 NodeModel 有个 Properties/Parameters 字段，这里示意解析：
            DataValue? value = null;
            try
            {
                // 尝试读取名为 "value" 的属性（具体取决于 NodeModel 定义）
                var prop = model.GetType().GetProperty("Properties");
                if (prop != null)
                {
                    var props = prop.GetValue(model);
                    // 这里假设 Properties 是 IDictionary<string, object>
                    if (props is System.Collections.IDictionary dict && dict.Contains("value"))
                        value = (DataValue)dict["value"];
                }
            }
            catch
            {
                value = null;
            }
            _value = value;
        }

        public async ValueTask ExecuteAsync(IRuntimeContext ctx)
        {
            await ctx.SetOutputAsync("value", _value);
            ctx.EmitTrace(new NodeLeaveTrace() { NodeId= Id });
        }
    }
}
