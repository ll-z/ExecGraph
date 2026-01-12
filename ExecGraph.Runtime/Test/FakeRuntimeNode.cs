using ExecGraph.Contracts.Common;
using ExecGraph.Contracts.Runtime;
using ExecGraph.Contracts.Trace;
//using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Runtime.Test
{
    public sealed class FakeRuntimeNode : IRuntimeNode
    {
        public NodeId Id { get; }

        public FakeRuntimeNode(NodeId id)
        {
            Id = id;
        }

        public void Execute(IRuntimeContext ctx)
        {
            //Log.Information($"[EXECUTE] Node {Id}");

            // 模拟一点工作
            Thread.Sleep(200);

            ctx.EmitTrace(new NodeEnterTrace());
        }
    }

    public sealed class NodeA : IRuntimeNode
    {
        public NodeId Id { get; }


        public NodeA(NodeId id) => Id = id;


        public void Execute(IRuntimeContext ctx)
        {
            ctx.SetOutput("out", 42);
           // Log.Information("NodeA -> out = 42");
            ctx.EmitTrace(new NodeEnterTrace());
        }
        
    }

    public sealed class NodeB : IRuntimeNode
    {
        public NodeId Id { get; }


        public NodeB(NodeId id) => Id = id;


        public void Execute(IRuntimeContext ctx)
        {
            var v = ctx.GetInput<int>("in");
           // Log.Information($"NodeB <- in = {v}");
            ctx.EmitTrace(new NodeEnterTrace());
        }
    }
}
