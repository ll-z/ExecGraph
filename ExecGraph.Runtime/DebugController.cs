using System;
using System.Collections.Generic;
using ExecGraph.Contracts.Common;
using ExecGraph.Contracts.Runtime;

namespace ExecGraph.Runtime.Debug
{
    public sealed class DebugController : IDebugController
    {
        private readonly HashSet<NodeId> _breakpoints = new();
        private readonly object _sync = new();

        public bool IsEnabled { get; set; } = true;

        public void AddBreakpoint(NodeId nodeId)
        {
            lock (_sync)
            {
                _breakpoints.Add(nodeId);
            }
        }

        public void RemoveBreakpoint(NodeId nodeId)
        {
            lock (_sync)
            {
                _breakpoints.Remove(nodeId);
            }
        }

        public void ClearBreakpoints()
        {
            lock (_sync)
            {
                _breakpoints.Clear();
            }
        }

        public bool ShouldBreak(NodeId nodeId)
        {
            if (!IsEnabled)
                return false;

            lock (_sync)
            {
                return _breakpoints.Contains(nodeId);
            }
        }
    }
}
