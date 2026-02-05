using ExecGraph.Builtins.Registration;
using ExecGraph.Contracts.Common;
using ExecGraph.Contracts.Data;
using ExecGraph.Contracts.Runtime;
using ExecGraph.Contracts.Trace;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExecGraph.Builtins.Nodes.Math
{
    [RuntimeNode("Multiply", Description = "Multiply two numeric inputs")]
    public sealed class MultiplyNode : IRuntimeNode
    {
        public NodeId Id { get; }

        public MultiplyNode(NodeId id) => Id = id;

        public async ValueTask ExecuteAsync(IRuntimeContext ctx)
        {
            // 尽量兼容整数/浮点输入：读取为 object 再转 double
            object? aObj = ctx.GetInput<object>("a");
            object? bObj = ctx.GetInput<object>("b");

            double a = ConvertToDouble(aObj);
            double b = ConvertToDouble(bObj);

            double product = a * b;

            // 如果你期望整数输出，可根据输入类型决定；这里统一输出 double
            //ctx.SetOutput("product", product);
            await ctx.SetOutputAsync("product", new DataValue(product, new DataTypeId("double")));
            ctx.EmitTrace(new NodeLeaveTrace() {NodeId =Id });
        }

        private static double ConvertToDouble(object? v)
        {
            if (v is null) return 0.0;
            if (v is double d) return d;
            if (v is float f) return f;
            if (v is int i) return i;
            if (v is long l) return l;
            if (v is decimal dec) return (double)dec;
            if (double.TryParse(v.ToString(), out var parsed)) return parsed;
            return 0.0;
        }
    }
}
