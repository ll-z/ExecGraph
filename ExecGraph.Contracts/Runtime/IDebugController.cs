using ExecGraph.Contracts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Contracts.Runtime
{
    public interface IDebugController
    {
        bool IsEnabled { get; }


        void AddBreakpoint(NodeId nodeId);
        void RemoveBreakpoint(NodeId nodeId);
        void ClearBreakpoints();


        bool ShouldBreak(NodeId nodeId);
    }
}
