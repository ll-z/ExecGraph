using ExecGraph.Contracts.Common;
using ExecGraph.Contracts.Runtime;
using ExecGraph.Contracts.Trace;
using ExecGraph.Runtime.Trace;

namespace ExecGraph.Runtime.VM
{
    public sealed class RuntimeContext : IRuntimeContext
    {
        private readonly NodeId _nodeId;
        private readonly DataStore _store;
        private readonly TraceEmitter _trace;
        public RunMode RunMode => RunMode.Automatic;

        internal RuntimeContext(NodeId nodeId, DataStore store, TraceEmitter trace)
        {
            _nodeId = nodeId;
            _store = store;
            _trace = trace;
        }


        public T GetInput<T>(string portName) => _store.GetInput<T>(_nodeId, portName);


        public void SetOutput<T>(string portName, T value) 
        {
            _store.SetOutput(_nodeId, portName, value);
            _trace.Emit(new DataWriteTrace
            {
                NodeId = _nodeId,
                Port = portName,
                Value = value
            });
        } 


        public void EmitTrace(TraceEvent trace)
        {
            _trace.Emit(trace);
            // 发布给 TraceEmitter
        }
    }
}
