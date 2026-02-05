using ExecGraph.Contracts.Common;
using ExecGraph.Contracts.Graph;
using ExecGraph.Contracts.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Builtins.Registration
{
    public delegate IRuntimeNode NodeFactory(NodeId id, NodeModel? model);

    public sealed class NodeTypeRegistry
    {
        private readonly ConcurrentDictionary<string, NodeFactory> _map = new();

        public void Register(string runtimeTypeName, NodeFactory factory) => _map[runtimeTypeName] = factory;

        public bool TryCreate(string runtimeTypeName, NodeId id, NodeModel? model, out IRuntimeNode? node)
        {
            if (_map.TryGetValue(runtimeTypeName, out var f))
            {
                node = f(id, model);
                return true;
            }

            node = null;
            return false;
        }
    }
}
