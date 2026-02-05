using ExecGraph.Abstractions.Common;
using ExecGraph.Contracts.Runtime;

namespace ExecGraph.Runtime
{
    public sealed class DebugController : IDebugController
    {
        private readonly HashSet<NodeId> _breakpoints = new();
        private readonly object _sync = new();

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get
            {
                lock (_sync)
                {
                    return _isEnabled;
                }
            }
        }

        public void Enable()
        {
            lock (_sync)
            {
                _isEnabled = true;
            }
        }

        public void Disable()
        {
            lock (_sync)
            {
                _isEnabled = false;
            }
        }

        public IReadOnlyCollection<NodeId> GetBreakpoints()
        {
            lock (_sync)
            {
                return _breakpoints.ToArray();
            }
        }

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
            

            lock (_sync)
            {
                return _isEnabled && _breakpoints.Contains(nodeId);
            }
        }
    }
}
