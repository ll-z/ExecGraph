using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Contracts.Common
{
    public readonly record struct NodeId(Guid Value)
    {
        public static NodeId New() => new(Guid.NewGuid());
        public override string ToString() => Value.ToString();
    }
}
