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
    /// Diagnostic wrapper that uses the token-based ExecutionController and
    /// exposes diagnostics for test consumption.
    /// If tests inject this controller into RuntimeHost, diagnostics will contain tokens.
    /// </summary>
    public sealed class ExecutionControllerDiag : ExecutionController
    {
        public readonly ConcurrentQueue<string> DiagOut = new();

        private static string Now() => DateTime.UtcNow.ToString("HH:mm:ss.ffff");

        private void D(string s)
        {
            try { DiagOut.Enqueue($"{Now()} [T{Thread.CurrentThread.ManagedThreadId}] {s}"); } catch { }
            // Also put into base Diagnostics (for compatibility)
            try { Diagnostics.Enqueue($"{Now()} [T{Thread.CurrentThread.ManagedThreadId}] {s}"); } catch { }
        }

        public override void SetRunMode(RunMode mode)
        {
            D($"SetRunMode({mode}) - enter");
            base.SetRunMode(mode);
            D($"SetRunMode({mode}) - exit");
        }

        public override void Run()
        {
            D("Run() - enter");
            base.Run();
            D("Run() - exit");
        }

        public override void Pause()
        {
            D("Pause() - enter");
            base.Pause();
            D("Pause() - exit");
        }

        public override void Step()
        {
            D("Step() - enter");
            base.Step();
            D("Step() - exit");
        }

        internal override void WaitIfNeeded()
        {
            D("WaitIfNeeded() - enter");
            base.WaitIfNeeded();
            D("WaitIfNeeded() - exit");
        }
    }
}
