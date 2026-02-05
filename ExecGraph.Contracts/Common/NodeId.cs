using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Contracts.Common
{
    //public readonly record struct NodeId(Guid Value)
    //{
    //    public static NodeId New() => new(Guid.NewGuid());
    //    public override string ToString() => Value.ToString();
    //}

    public readonly struct NodeId : IEquatable<NodeId>
    {
        private readonly Guid _g;
        public NodeId(Guid g) => _g = g;
        public static NodeId New() => new NodeId(Guid.NewGuid());
        public override string ToString() => _g.ToString("D");
        public bool Equals(NodeId other) => _g.Equals(other._g);
        public override bool Equals(object? obj) => obj is NodeId o && Equals(o);
        public override int GetHashCode() => _g.GetHashCode();
        public static bool operator ==(NodeId a, NodeId b) => a.Equals(b);
        public static bool operator !=(NodeId a, NodeId b) => !a.Equals(b);
    }
}
