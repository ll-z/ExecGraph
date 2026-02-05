

using ExecGraph.Abstractions.Common;

namespace ExecGraph.Runtime.Abstractions.Runtime
{
    public interface IRuntimeNode
    {
        NodeId Id { get; }
        ValueTask ExecuteAsync(IRuntimeContext ctx);
    }
}
