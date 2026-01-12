using ExecGraph.Contracts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Contracts.Trace
{
    public sealed record FlowTrace(NodeId From, NodeId To) : TraceEvent;
}
