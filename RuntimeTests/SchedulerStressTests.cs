// Tests/RuntimeTests/SchedulerStressTests.cs

using System.Collections.Concurrent;

using Xunit.Abstractions;

using ExecGraph.Contracts.Graph;

using ExecGraph.Runtime;
using ExecGraph.Runtime.Execution;
using ExecGraph.Abstractions.Common;
using ExecGraph.Runtime.Abstractions.Runtime;
using ExecGraph.Abstractions.Trace;

namespace RuntimeTests
{


    public class SchedulerStressTests : IDisposable
    {
        private readonly ITestOutputHelper _out;
        public SchedulerStressTests(ITestOutputHelper output) => _out = output;

        public void Dispose() { }

        // Helper: writes diagnostics either to output or to a temp file if too long
        private void DumpDiagnostics(ExecutionControllerDiag diag)
        {
            if (diag == null) return;
            var list = diag.DiagOut.ToArray();
            if (list.Length <= 1000)
            {
                _out.WriteLine("=== ExecutionController DiagOut ===");
                foreach (var line in list) _out.WriteLine(line);
                _out.WriteLine("=== End DiagOut ===");
            }
            else
            {
                var path = Path.Combine(Path.GetTempPath(), $"diag_{Guid.NewGuid()}.log");
                File.WriteAllLines(path, list);
                _out.WriteLine($"DiagOut too long ({list.Length} lines). Written to: {path}");
            }
        }

        [Fact(DisplayName = "Stress: multiple threads calling Step rapidly should not lose executions")]
        public async Task Stress_Steps_NoMissedExecutions()
        {
            // Arrange
            const int nodeCount = 120;
            const int threads = 8;
            const int stepsPerThread = 20; // total 160 > nodeCount (saturation)
            const int waitMs = 10000;
            var nodeIds = Enumerable.Range(0, nodeCount).Select(_ => NodeId.New()).ToArray();

            var graph = new GraphModel
            {
                Nodes = nodeIds.Select(id => new NodeModel
                {
                    Id = id,
                    RuntimeType = typeof(FakeRuntimeNode).AssemblyQualifiedName!
                }).ToArray(),
                Links = Array.Empty<LinkModel>()
            };

            var runtimeNodes = nodeIds.Select(id => (IRuntimeNode)new FakeRuntimeNode(id, id.ToString(), workMs: 10)).ToArray();

            // inject diag controller so we have diagnostics and token semantics
            var diagCtrl = new ExecutionControllerDiag();
            var debug = new DebugController();
            var host = new RuntimeHost(graph, runtimeNodes, diagCtrl, debug);
            var controller = host.Controller;

            // For collecting NodeEnter traces
            var entered = new ConcurrentBag<NodeId>();
            var tcsAll = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            host.Trace.TracePublished += (tr) =>
            {
                if (tr is NodeEnterTrace ne)
                {
                    entered.Add(ne.NodeId);
                    if (entered.Distinct().Count() >= nodeCount)
                        tcsAll.TrySetResult(true);
                }
            };

            // Counters / synchronization for Step callers
            int stepCalls = 0;
            var startGate = new CountdownEvent(threads);    // indicate all threads are ready
            var go = new ManualResetEventSlim(false);       // start signal

            // Start runtime
            controller.SetRunMode(RunMode.Development);
            var rtThread = new Thread(host.Start) { IsBackground = true };
            rtThread.Start();

            try
            {
                // create tasks that will do Steps concurrently
                var tasks = new Task[threads];
                for (int t = 0; t < threads; t++)
                {
                    tasks[t] = Task.Run(() =>
                    {
                        // each thread signals readiness then waits for go
                        startGate.Signal();
                        startGate.Wait(); // ensure all threads reach here
                        // Now step loop
                        for (int i = 0; i < stepsPerThread; i++)
                        {
                            Interlocked.Increment(ref stepCalls); // atomic count
                            controller.Step();
                            // small jitter helps interleaving
                            Thread.SpinWait(50);
                        }
                    });
                }

                // wait for threads to be ready, then let them start
                startGate.Wait();
                // Give runtime a tiny moment to settle
                Thread.Sleep(30);
                // Release tasks (they effectively start immediately since we used startGate.Wait)
                // Wait tasks to finish issuing Steps
                await Task.WhenAll(tasks).ConfigureAwait(false);

                _out.WriteLine($"All Step tasks completed. Total Step calls attempted by all threads: {stepCalls}");

                // Now wait for scheduler to execute all nodes or timeout
                var completedTask = await Task.WhenAny(tcsAll.Task, Task.Delay(waitMs));
                Assert.True(completedTask == tcsAll.Task, $"Timeout: not all nodes executed within {waitMs}ms");

                // Validate unique nodes executed equals nodeCount
                var uniqueEntered = entered.Distinct().Count();
                Assert.Equal(nodeCount, uniqueEntered);
            }
            finally
            {
                // ensure host stopped
                try { controller.Run(); } catch { }
                try { host.Stop(); } catch { }

                // Dump diag (may be large)
                if (host.Controller is ExecutionControllerDiag diag) DumpDiagnostics(diag);
            }
        }

        [Fact(DisplayName = "Stress: mixed Step/Run calls from multiple threads remain stable")]
        public async Task Stress_MixedStepAndRun_Stability()
        {
            // Arrange
            const int nodeCount = 150;
            const int stepThreads = 10;
            const int stepsPerThread = 12;
            const int waitMs = 15000;
            var nodeIds = Enumerable.Range(0, nodeCount).Select(_ => NodeId.New()).ToArray();

            var graph = new GraphModel
            {
                Nodes = nodeIds.Select(id => new NodeModel
                {
                    Id = id,
                    RuntimeType = typeof(FakeRuntimeNode).AssemblyQualifiedName!
                }).ToArray(),
                Links = Array.Empty<LinkModel>()
            };

            var runtimeNodes = nodeIds.Select(id => (IRuntimeNode)new FakeRuntimeNode(id, id.ToString(), workMs: 8)).ToArray();

            var diagCtrl = new ExecutionControllerDiag();
            var debug = new DebugController();
            var host = new RuntimeHost(graph, runtimeNodes, diagCtrl, debug);
            var controller = host.Controller;

            var entered = new ConcurrentBag<NodeId>();
            var tcsAll = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            host.Trace.TracePublished += (tr) =>
            {
                if (tr is NodeEnterTrace ne)
                {
                    entered.Add(ne.NodeId);
                    if (entered.Distinct().Count() >= nodeCount)
                        tcsAll.TrySetResult(true);
                }
            };

            int stepCalls = 0;
            var startGate = new CountdownEvent(stepThreads);

            controller.SetRunMode(RunMode.Development);
            var rtThread = new Thread(host.Start) { IsBackground = true };
            rtThread.Start();

            try
            {
                var stepTasks = new Task[stepThreads];
                for (int t = 0; t < stepThreads; t++)
                {
                    stepTasks[t] = Task.Run(() =>
                    {
                        startGate.Signal();
                        startGate.Wait();
                        for (int i = 0; i < stepsPerThread; i++)
                        {
                            Interlocked.Increment(ref stepCalls);
                            controller.Step();
                            Thread.SpinWait(50);
                        }
                    });
                }

                // ensure all step threads ready
                startGate.Wait();
                // let some steps happen, then fire Run() concurrently
                await Task.Delay(200).ConfigureAwait(false);

                var runTask = Task.Run(() =>
                {
                    controller.Run(); // switch to continuous
                });

                // wait for all tasks to finish
                await Task.WhenAll(stepTasks).ConfigureAwait(false);
                await runTask.ConfigureAwait(false);

                _out.WriteLine($"Total Step calls attempted by all threads: {stepCalls}");

                // wait for completion
                var completed = await Task.WhenAny(tcsAll.Task, Task.Delay(waitMs));
                Assert.True(completed == tcsAll.Task, $"Timeout: not all nodes executed within {waitMs}ms");

                var uniqueExecuted = entered.Distinct().Count();
                Assert.Equal(nodeCount, uniqueExecuted);
            }
            finally
            {
                try { controller.Run(); } catch { }
                try { host.Stop(); } catch { }

                if (host.Controller is ExecutionControllerDiag diag) DumpDiagnostics(diag);
            }
        }
    }
}
