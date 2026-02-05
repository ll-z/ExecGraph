

using ExecGraph.Abstractions.Common;
using ExecGraph.Abstractions.Trace;
using ExecGraph.Runtime.Abstractions.Runtime;

namespace RuntimeTests
{
    public class FakeRuntimeNode : IRuntimeNode
    {
        public NodeId Id { get; }
        private readonly string _label;
        private readonly int _workMs;

        public FakeRuntimeNode(NodeId id, string label = null, int workMs = 20)
        {
            Id = id;
            _label = label ?? id.ToString();
            _workMs = workMs;
        }

        // <-- EXACT signature required by Contracts
        public async ValueTask ExecuteAsync(IRuntimeContext ctx)
        {
            // Emit enter/leave traces so tests can observe behavior
            ctx.EmitTrace(new NodeEnterTrace { NodeId = Id });
            Thread.Sleep(_workMs);
            ctx.EmitTrace(new NodeLeaveTrace { NodeId = Id });
        }
    }
}
