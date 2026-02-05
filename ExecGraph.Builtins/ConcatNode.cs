using ExecGraph.Contracts.Common;
using ExecGraph.Contracts.Runtime;
using ExecGraph.Contracts.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Builtins
{
    public class ConcatNode : IRuntimeNode
    {
        public NodeId Id { get; }
        public ConcatNode(NodeId id) => Id = id;

        public void Execute(IRuntimeContext ctx)
        {
            ctx.EmitTrace(new NodeEnterTrace { NodeId = Id });

            string? s1 = ctx.GetInput<string>("left");
            string? s2 = ctx.GetInput<string>("right");

            // 注意处理 null（GetInput 在无值时返回 default 即 null）
            var result = (s1 ?? "") + (s2 ?? "");
            ctx.SetOutput("out", result);

            ctx.EmitTrace(new NodeLeaveTrace { NodeId = Id });
        }
    }
}
