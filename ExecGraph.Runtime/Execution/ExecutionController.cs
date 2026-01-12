using ExecGraph.Contracts.Common;
using ExecGraph.Contracts.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Runtime.Execution
{
    /// <summary>
    /// ExecutionController based on explicit token queue (sequence numbers).
    /// Step() enqueues a token (seq); WaitIfNeeded consumes a token.
    /// SetRunMode(Pause) clears tokens. Diagnostics available in Diagnostics queue.
    /// </summary>
    public class ExecutionController : IExecutionController, IDisposable
    {
        private readonly object _sync = new();
        private readonly Queue<long> _tokenQueue = new();
        private long _nextSeq = 0;
        private bool _isContinuous = true;
        public RunMode RunMode { get; private set; } = RunMode.Automatic;

        // Diagnostics queue (thread-safe) - tests/diag can read this.
        public readonly ConcurrentQueue<string> Diagnostics = new();

        private static string Now() => DateTime.UtcNow.ToString("HH:mm:ss.ffff");
        private void Log(string s)
        {
            try { Diagnostics.Enqueue($"{Now()} [T{Thread.CurrentThread.ManagedThreadId}] {s}"); } catch { }
        }

        public virtual void SetRunMode(RunMode mode)
        {
            lock (_sync)
            {
                Log($"SetRunMode({mode}) enter");

                if (RunMode == mode)
                    return;

                RunMode = mode;
                if (mode == RunMode.Automatic)
                {
                    _isContinuous = true;
                    Monitor.PulseAll(_sync);
                }
                else
                {
                    _isContinuous = false;
                    //_tokenQueue.Clear();
                }
                Log($"SetRunMode({mode}) exit");
            }
        }

        public virtual void Run()
        {
            lock (_sync)
            {
                Log("Run() enter");
                _isContinuous = true;
                Monitor.PulseAll(_sync);
                Log("Run() exit");
            }
        }

        public virtual void Pause()
        {
            lock (_sync)
            {
                Log("Pause() enter");
                _isContinuous = false;
                _tokenQueue.Clear();
                Log("Pause() exit (tokens cleared)");
            }
        }

        public virtual void Step()
        {
            long seq;
            lock (_sync)
            {
                Log("Step() enter");
                if (RunMode == RunMode.Automatic)
                {
                    _isContinuous = true;
                    Monitor.PulseAll(_sync);
                    Log("Step() treated as Run because RunMode==Automatic");
                    return;
                }

                if (_isContinuous)
                {
                    Log("Step() no-op because already continuous");
                    return;
                }

                seq = ++_nextSeq;
                _tokenQueue.Enqueue(seq);
                Monitor.PulseAll(_sync);
                Log($"Step() enqueued token seq={seq} (queueSize={_tokenQueue.Count})");
            }
        }

        /// <summary>
        /// The single blocking point in Scheduler: waits for continuous or a token, then consumes one token.
        /// </summary>
        internal virtual void WaitIfNeeded()
        {
            if (RunMode == RunMode.Automatic) { Log("WaitIfNeeded() fastpath: Automatic"); return; }

            lock (_sync)
            {
                Log($"WaitIfNeeded() enter (isContinuous={_isContinuous}, queueSize={_tokenQueue.Count})");
                while (!_isContinuous && _tokenQueue.Count == 0)
                {
                    Monitor.Wait(_sync);
                }

                if (_isContinuous)
                {
                    Log("WaitIfNeeded() awakened by continuous");
                    return;
                }

                var seq = _tokenQueue.Dequeue();
                Log($"WaitIfNeeded() consumed token seq={seq} (queueSize={_tokenQueue.Count})");
            }
        }

        public virtual void Dispose()
        {
            // nothing special
        }
    }
}
