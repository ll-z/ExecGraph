using ExecGraph.Contracts.Common;
using ExecGraph.Contracts.Graph;
using ExecGraph.Contracts.Runtime;
using ExecGraph.Runtime.Debug;
using ExecGraph.Runtime.Execution;

namespace ExecGraph.Runtime
{
    public sealed class RuntimeHost
    {
        private readonly GraphModel _graph;
        private readonly IEnumerable<IRuntimeNode> _runtimeNodes;
        private ExecutionState _state = ExecutionState.Idle;

        private Scheduler.Scheduler _scheduler;
        private ExecutionController _controller;
        private DebugController _debug;

        private Thread? _runtimeThread;
        private CancellationTokenSource? _cts;
        private readonly object _sync = new();

        private NodeId? _currentStartNode;
        private NodeId? _pendingStartNode;
        private bool _isRunning;

        // Default constructor - constructs its own controller/debug
        public RuntimeHost(GraphModel graph, IEnumerable<IRuntimeNode> runtimeNodes)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _runtimeNodes = runtimeNodes ?? throw new ArgumentNullException(nameof(runtimeNodes));

            _controller = new ExecutionController();
            _debug = new DebugController();
            _scheduler = new Scheduler.Scheduler(_graph, _runtimeNodes, _controller, _debug, startNode: null);
        }

        // Overload: allow injection of controller/debug for testing/diagnostics
        public RuntimeHost(GraphModel graph, IEnumerable<IRuntimeNode> runtimeNodes, ExecutionController controller, DebugController debug)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _runtimeNodes = runtimeNodes ?? throw new ArgumentNullException(nameof(runtimeNodes));

            _controller = controller ?? new ExecutionController();
            _debug = debug ?? new DebugController();
            _scheduler = new Scheduler.Scheduler(_graph, _runtimeNodes, _controller, _debug, startNode: null);
        }

        public ExecutionController Controller => _controller;
        public DebugController Debug => _debug;
        public Trace.TraceEmitter Trace => _scheduler.Trace;

        public bool IsRunning { get { lock (_sync) { return _isRunning; } } }

        public StartNodeChangeResult TrySetStartNode(NodeId? startNode)
        {
            lock (_sync)
            {

      
                if (NullableEquals(_currentStartNode, startNode) && _pendingStartNode == null)
                    return StartNodeChangeResult.Applied;

                if (!_isRunning)
                {
                    _currentStartNode = startNode;
                    RebuildScheduler(startNode);
                    return StartNodeChangeResult.Applied;
                }
                else
                {
                    _pendingStartNode = startNode;
                    _state = ExecutionState.PendingRestart;
                    return StartNodeChangeResult.RequireRestartConfirm;
                }

                if (_controller.RunMode != RunMode.Development)
                    return StartNodeChangeResult.Rejected;
            }
        }

        public void RejectPendingStartNode()
        {
            lock (_sync) 
            { 
                _pendingStartNode = null;
                _state = ExecutionState.Idle;
            }
        }

        public void ConfirmPendingStartNodeAndRestart()
        {
            NodeId? pending;
            lock (_sync)
            {
                pending = _pendingStartNode;
                if (pending == null) return;
                _pendingStartNode = null;
            }

            _controller.Pause();
            StopScheduler();
            lock (_sync)
            {
                _currentStartNode = pending;
                RebuildScheduler(_currentStartNode);
                _state = ExecutionState.Idle;
            }
            StartScheduler();
        }

        private void RebuildScheduler(NodeId? startNode)
        {
            _scheduler = new Scheduler.Scheduler(_graph, _runtimeNodes, _controller, _debug, startNode);
        }

   

        private void StartScheduler()
        {
            lock (_sync)
            {
                if (_isRunning) return;
                _cts = new CancellationTokenSource();
                _runtimeThread = new Thread(() =>
                {
                    try
                    {
                        _isRunning = true;
                        _scheduler.Run(_cts.Token);
                    }
                    catch (OperationCanceledException) { }
                    finally { lock (_sync) { _isRunning = false; } }
                })
                { IsBackground = true };
                _runtimeThread.Start();
            }
        }

        private void StopScheduler()
        {
            lock (_sync)
            {
                if (!_isRunning) return;
                _cts?.Cancel();
            }

            _runtimeThread?.Join();

            lock (_sync)
            {
                _cts?.Dispose();
                _cts = null;
                _runtimeThread = null;
                _isRunning = false;
            }
        }


        public void Start() => StartScheduler();
        public void Stop() => StopScheduler();

        public StartNodeChangeResult ResetToInitial() => TrySetStartNode(null);

        private static bool NullableEquals(NodeId? a, NodeId? b)
        {
            if (!a.HasValue && !b.HasValue) return true;
            if (a.HasValue != b.HasValue) return false;
            return a!.Value.Equals(b!.Value);
        }
    }
}
