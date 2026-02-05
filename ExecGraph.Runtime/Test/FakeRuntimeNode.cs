
using ExecGraph.Abstractions.Common;
using ExecGraph.Abstractions.Data;
using ExecGraph.Abstractions.Trace;
using ExecGraph.Contracts.Trace;
using ExecGraph.Runtime.Abstractions.Runtime;


namespace ExecGraph.Runtime.Test
{
    public sealed class FakeRuntimeNode : IRuntimeNode
    {
        public NodeId Id { get; }

        public FakeRuntimeNode(NodeId id)
        {
            Id = id;
        }

        public async ValueTask ExecuteAsync(IRuntimeContext ctx)
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


        public async ValueTask ExecuteAsync(IRuntimeContext ctx)
        {
            //ctx.SetOutput("out", 42);
            await ctx.SetOutputAsync("out", new DataValue(42, new DataTypeId("int")));
            // Log.Information("NodeA -> out = 42");
            ctx.EmitTrace(new NodeEnterTrace());
        }
        
    }

    public sealed class NodeB : IRuntimeNode
    {
        public NodeId Id { get; }


        public NodeB(NodeId id) => Id = id;


        public async ValueTask ExecuteAsync(IRuntimeContext ctx)
        {
            var v = ctx.GetInput<int>("in");
           // Log.Information($"NodeB <- in = {v}");
            ctx.EmitTrace(new NodeEnterTrace());
        }
    }
}
