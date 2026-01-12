using ExecGraph.Contracts.Graph;

namespace ExecGraph.Contracts.Runtime
{
    public interface IRuntimeNodeFactory
    {
        IRuntimeNode Create(NodeModel model);
    }
}
