using ExecGraph.Contracts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Contracts.Runtime
{
    public interface IRuntimeNode
    {
        NodeId Id { get; }
        void Execute(IRuntimeContext context);
    }
}
