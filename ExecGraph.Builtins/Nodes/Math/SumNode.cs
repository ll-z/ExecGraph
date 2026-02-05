

using ExecGraph.Abstractions.Common;
using ExecGraph.Abstractions.Data;
using ExecGraph.Abstractions.Trace;
using ExecGraph.Runtime.Abstractions.Runtime;

namespace ExecGraph.Builtins.Nodes.Math
{
    [RuntimeNode("Sum")]
    public sealed class SumNode : IRuntimeNode
    {
        public NodeId Id { get; }

        // 约定：使用 NodeId 构造
        public SumNode(NodeId id) => Id = id;

        public async ValueTask ExecuteAsync(IRuntimeContext ctx)
        {
            // 发 enter trace（方便测试 / 调试）
            ctx.EmitTrace(new NodeEnterTrace { NodeId = Id });

            var a = ctx.GetInput<int>("a");
            var b = ctx.GetInput<int>("b");
            var sum = a + b;

            // SetOutput 将把值路由到所有下游并自动发布 DataWriteTrace（RuntimeContext 实现负责）
            //ctx.SetOutput("sum", sum);
            await ctx.SetOutputAsync("sum", new DataValue(sum, new DataTypeId("int")));
            ctx.EmitTrace(new NodeLeaveTrace { NodeId = Id });
        }
    }
}
