using ExecGraph.Abstractions.Common;
using ExecGraph.Contracts.Graph;
using ExecGraph.Runtime.Abstractions.Runtime;

namespace ExecGraph.Builtins.Registration
{
    public delegate IRuntimeNode NodeFactory(NodeId id, NodeModel model);

    public sealed class NodeTypeRegistry
    {
        private readonly Dictionary<string, NodeFactory> _map = new();
        public void Register(string runtimeTypeName, NodeFactory factory) => _map[runtimeTypeName] = factory;
        public bool TryCreate(string runtimeTypeName, NodeModel model, out IRuntimeNode node)
        {
            node = null!;
            if (_map.TryGetValue(runtimeTypeName, out var f))
            {
                node = f(model.Id, model);
                return true;
            }
            return false;
        }
    }
}
