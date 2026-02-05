using ExecGraph.Builtins.Registration;
using ExecGraph.Contracts.Common;
using ExecGraph.Contracts.Data;
using ExecGraph.Contracts.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

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
            ctx.EmitTrace(new ExecGraph.Contracts.Trace.NodeLeaveTrace() { NodeId=Id});
        }
    }
}
