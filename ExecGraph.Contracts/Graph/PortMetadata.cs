using ExecGraph.Contracts.Common;
using ExecGraph.Contracts.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Contracts.Graph
{
    public sealed class PortMetadata
    {
        public required string Name { get; init; }
        public required PortDirection Direction { get; init; }
        public required PortKind Kind { get; init; }
        public required DataTypeId DataType { get; init; }
        public bool IsSingle { get; init; }
    }

    public sealed class GraphModel
    {
        public IReadOnlyList<NodeModel> Nodes { get; init; } = Array.Empty<NodeModel>();
        public IReadOnlyList<LinkModel> Links { get; init; } = Array.Empty<LinkModel>();
    }


    public sealed class NodeModel
    {
        public required NodeId Id { get; init; }
        public required string RuntimeType { get; init; }
        public IReadOnlyList<PortMetadata> Ports { get; init; } = Array.Empty<PortMetadata>();
    }


    public sealed class LinkModel
    {
        public required NodeId FromNode { get; init; }
        public required string FromPort { get; init; }
        public required NodeId ToNode { get; init; }
        public required string ToPort { get; init; }
    }
}
