using ExecGraph.Builtins.Registration;
using ExecGraph.Contracts.Common;
using ExecGraph.Contracts.Runtime;
using ExecGraph.Contracts.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Builtins.Nodes.Math
{
    [RuntimeNode("Sum")]
    public sealed class SumNode : IRuntimeNode
    {
        public NodeId Id { get; }

        // 约定：使用 NodeId 构造
        public SumNode(NodeId id) => Id = id;

        public void Execute(IRuntimeContext ctx)
        {
            // 发 enter trace（方便测试 / 调试）
            ctx.EmitTrace(new NodeEnterTrace { NodeId = Id });

            var a = ctx.GetInput<int>("a");
            var b = ctx.GetInput<int>("b");
            var sum = a + b;

            // SetOutput 将把值路由到所有下游并自动发布 DataWriteTrace（RuntimeContext 实现负责）
            ctx.SetOutput("sum", sum);

            ctx.EmitTrace(new NodeLeaveTrace { NodeId = Id });
        }
    }
}
