using ExecGraph.Contracts.Common;
using ExecGraph.Contracts.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Runtime.Test
{
    public static class TestGraphFactory
    {
        public static GraphModel CreateSingleNodeGraph(NodeId nodeId)
        {
            return new GraphModel
            {
                Nodes = new[]
                {
                new NodeModel
                {
                    Id = nodeId,
                    RuntimeType = typeof(FakeRuntimeNode).AssemblyQualifiedName!,
                    Ports = Array.Empty<PortMetadata>()
                }
            },
                Links = Array.Empty<LinkModel>()
            };
        }
    }
}
