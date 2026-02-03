using ExecGraph.Contracts.Common;
using ExecGraph.Contracts.Graph;
using ExecGraph.Contracts.Runtime;
using ExecGraph.Contracts.Trace;
using ExecGraph.Runtime.Execution;
using ExecGraph.Runtime.Trace;
using ExecGraph.Runtime.VM;

namespace ExecGraph.Runtime.Scheduler
{
    public sealed class Scheduler
    {
        private readonly Dictionary<NodeId, NodeRuntimeState> _nodes = new();
        private readonly Queue<NodeRuntimeState> _readyQueue = new();
        private readonly ExecutionController _controller;
        private readonly DataStore _dataStore;
        private readonly TraceEmitter _trace;
        private readonly DebugController _debug;
        private readonly HashSet<NodeId> _activeNodes = new();

        public TraceEmitter Trace => _trace;

        public Scheduler(GraphModel graph,
                         IEnumerable<IRuntimeNode> runtimeNodes,
                         ExecutionController controller,
                         DebugController debug,
                         NodeId? startNode = null)
        {
            _controller = controller;
            _debug = debug;
            _dataStore = new DataStore(graph);
            _trace = new TraceEmitter();

            foreach (var node in runtimeNodes)
                _nodes[node.Id] = new NodeRuntimeState(node.Id, node);

            if (graph?.Links != null)
            {
                foreach (var link in graph.Links)
                {
                    var from = _nodes[link.FromNode];
                    var to = _nodes[link.ToNode];
                    from.Outgoing.Add(to.Id);
                }
            }

            if (startNode.HasValue)
                BuildActiveSet(startNode.Value);
            else
                foreach (var id in _nodes.Keys) _activeNodes.Add(id);

            foreach (var node in _nodes.Values)
            {
                if (!_activeNodes.Contains(node.Id)) continue;
                foreach (var next in node.Outgoing)
                {
                    if (_activeNodes.Contains(next))
                        _nodes[next].InDegree++;
                }
            }

            foreach (var node in _nodes.Values)
            {
                if (_activeNodes.Contains(node.Id) && node.InDegree == 0)
                    _readyQueue.Enqueue(node);
            }
        }

        private void BuildActiveSet(NodeId start)
        {
            var stack = new Stack<NodeId>();
            stack.Push(start);
            while (stack.Count > 0)
            {
                var id = stack.Pop();
                if (!_activeNodes.Add(id)) continue;
                foreach (var next in _nodes[id].Outgoing)
                    stack.Push(next);
            }
        }

        public void Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _readyQueue.Count > 0)
            {
                var current = _readyQueue.Dequeue();

                if (!_activeNodes.Contains(current.Id))
                    continue;

                // *** Scheduler diagnostic log BEFORE wait ***
                Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss.ffff} [Scheduler] Waiting permission for node {current.Id}");

                // SINGLE BLOCKING POINT
                _controller.WaitIfNeeded();

                // After wait, log continuing
                Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss.ffff} [Scheduler] Proceeding to execute node {current.Id}");

                _trace.Emit(new ExecGraph.Contracts.Trace.NodeEnterTrace { NodeId = current.Id });

                // Debug breakpoint
                if (_controller.RunMode == RunMode.Development && _debug.ShouldBreak(current.Id))
                {
                    _controller.Pause();
                }

                // Only wait again if paused by debug (consistent with earlier fix)
                if (_controller.RunMode == RunMode.Development && _debug.ShouldBreak(current.Id))
                {
                    _controller.WaitIfNeeded();
                }

                current.Node.Execute(new RuntimeContext(current.Id, _dataStore, _trace, _controller.RunMode));

                _trace.Emit(new ExecGraph.Contracts.Trace.NodeLeaveTrace { NodeId = current.Id });

                if (token.IsCancellationRequested) break;

                foreach (var nextId in current.Outgoing)
                {
                    if (!_activeNodes.Contains(nextId)) continue;
                    _trace.Emit(new FlowTrace(current.Id, nextId));
                    var next = _nodes[nextId];
                    next.InDegree--;
                    if (next.InDegree == 0)
                        _readyQueue.Enqueue(next);
                }
            }
        }
    }

    internal sealed class NodeRuntimeState
    {
        public NodeId Id { get; }
        public IRuntimeNode Node { get; }
        public int InDegree;
        public List<NodeId> Outgoing = new();
        public NodeRuntimeState(NodeId id, IRuntimeNode node)
        {
            Id = id;
            Node = node;
        }
    }
}
