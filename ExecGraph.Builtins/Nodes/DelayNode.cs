

using ExecGraph.Abstractions.Common;
using ExecGraph.Abstractions.Data;
using ExecGraph.Abstractions.Trace;
using ExecGraph.Runtime.Abstractions.Runtime;

namespace ExecGraph.Builtins.Nodes
{
    [RuntimeNode("Delay", Description = "Sleep for a short duration (ms) then emit done=true)")]
    public sealed class DelayNode : IRuntimeNode
    {
        public NodeId Id { get; }
        private readonly int _ms;

        public DelayNode(NodeId id, int ms = 100)
        {
            Id = id;
            _ms = ms;
        }

        public async ValueTask ExecuteAsync(IRuntimeContext ctx)
        {
            Thread.Sleep(_ms);

            await ctx.SetOutputAsync("done", new DataValue(true, new DataTypeId("bool")));
            ctx.EmitTrace(new NodeLeaveTrace() { NodeId=Id});
        }
    }
}
