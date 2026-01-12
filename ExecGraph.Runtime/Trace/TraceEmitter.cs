using ExecGraph.Contracts.Trace;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Runtime.Trace
{
    //显示执行状态
    public sealed class TraceEmitter
    {
        private readonly ConcurrentQueue<TraceEvent> _buffer = new();


        public event Action<TraceEvent>? TracePublished;


        public void Emit(TraceEvent trace)
        {
            if (trace == null) return;
            _buffer.Enqueue(trace);
            try { TracePublished?.Invoke(trace); } catch { /* swallow */ }
        }


        public IEnumerable<TraceEvent> Drain()
        {
            while (_buffer.TryDequeue(out var e))
                yield return e;
        }
    }
}
