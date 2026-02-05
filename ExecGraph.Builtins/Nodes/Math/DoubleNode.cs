using ExecGraph.Contracts.Common;
using ExecGraph.Contracts.Data;
using ExecGraph.Contracts.Runtime;
using ExecGraph.Contracts.Trace;

namespace ExecGraph.Builtins.Nodes.Math
{
    public class DoubleNode : IRuntimeNode
    {
        public NodeId Id { get; }

        public DoubleNode(NodeId id) => Id = id;

        // 必须精确匹配 Contracts 中的签名
        public async ValueTask ExecuteAsync(IRuntimeContext ctx)
        {
            // 发出进入 trace（可选，方便调试 / 测试）
            ctx.EmitTrace(new NodeEnterTrace { NodeId = Id });

            // 读取名为 "in" 的端口，如果没有值则得到 default(int)
            var input = ctx.GetInput<int>("in");

            // 做事：把输入乘以 2
            var output = input * 2;

            // 写出到名为 "out" 的输出端口
            // RuntimeContext.SetOutput 会负责把值路由到下游输入且自动发 DataWriteTrace
            //ctx.SetOutput("out", output);
            await ctx.SetOutputAsync("out", new DataValue(output, new DataTypeId("int")));

            // 发出离开 trace（可选）
            ctx.EmitTrace(new NodeLeaveTrace { NodeId = Id });
        }
    }
}
