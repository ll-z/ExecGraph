using ExecGraph.Contracts.Graph;
using ExecGraph.Runtime.Abstractions.Runtime;

namespace ExecGraph.Contracts.Runtime
{
    public interface IRuntimeNodeFactory
    {
        IRuntimeNode Create(NodeModel model);
    }
}
