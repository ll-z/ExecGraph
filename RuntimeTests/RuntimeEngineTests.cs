using System.Collections.Concurrent;
using ExecGraph.Contracts.Common;
using ExecGraph.Contracts.Data;
using ExecGraph.Contracts.Graph;
using ExecGraph.Contracts.Trace;
using ExecGraph.Contracts.Runtime;
using ExecGraph.Runtime;
using ExecGraph.Builtins.Nodes.Math;

namespace RuntimeTests
{
    // Fake runtime node implementing the exact IRuntimeNode interface.


    public class RuntimeEngineTests : IDisposable
    {
        private const int WaitMs = 5000;

        public void Dispose()
        {
        }

        [Fact(DisplayName = "Development Step/Run: Step executes single ready node, Run executes remaining")]
        public async Task StepAndRun_Development_AllNodesExecutedInOrder()
        {
            var idA = NodeId.New();
            var idB = NodeId.New();
            var idC = NodeId.New();

            var graph = new GraphModel
            {
                Nodes =
                [
                    new NodeModel
                    {
                        Id = idA,
                        RuntimeType = typeof(FakeRuntimeNode).AssemblyQualifiedName!,
                        Ports =
                        [
                            new PortMetadata
                            {
                                Name = "out",
                                Direction = PortDirection.Output,
                                Kind = PortKind.Data,
                                DataType = new DataTypeId("any"),
                                IsSingle = false
                            }
                        ]
                    },
                    new NodeModel
                    {
                        Id = idB,
                        RuntimeType = typeof(FakeRuntimeNode).AssemblyQualifiedName!,
                        Ports =
                        [
                            new PortMetadata
                            {
                                Name = "in",
                                Direction = PortDirection.Input,
                                Kind = PortKind.Data,
                                DataType = new DataTypeId("any"),
                                IsSingle = false
                            }
                        ]
                    },

                    new NodeModel
                    {
                        Id = idC,
                        RuntimeType = typeof(DoubleNode).AssemblyQualifiedName!,
                        Ports =
                        [
                            new PortMetadata
                            {
                                Name = "in",
                                Direction = PortDirection.Input,
                                Kind = PortKind.Data,
                                DataType = new DataTypeId("any"),
                                IsSingle = false
                            }
                        ]
                    },

                ],
                Links = new[]
                {
                    new LinkModel { FromNode = idA, FromPort = "out", ToNode = idB, ToPort = "in" },
                    new LinkModel { FromNode = idA, FromPort = "out", ToNode = idC, ToPort = "in" }
                }
            };

            var nodes = new IRuntimeNode[]
            {
                new FakeRuntimeNode(idA, "A", 50),
                new FakeRuntimeNode(idB, "B", 50),
                 new DoubleNode(idC)
            };

            var host = new RuntimeHost(graph, nodes);
            var controller = host.Controller;
            var debug = host.Debug;

            var tcsA = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var tcsB = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            host.Trace.TracePublished += (tr) =>
            {
                if (tr is NodeEnterTrace ent && ent.NodeId.Equals(idA)) tcsA.TrySetResult(true);
                if (tr is NodeEnterTrace ent2 && ent2.NodeId.Equals(idB)) tcsB.TrySetResult(true);
            };

            controller.SetRunMode(RunMode.Development);

            var rtThread = new Thread(host.Start) { IsBackground = true };
            rtThread.Start();

            // Step A
            controller.Step();
            var aTask = await Task.WhenAny(tcsA.Task, Task.Delay(WaitMs));
            Assert.True(aTask == tcsA.Task, "Node A did not execute on STEP");

            // Step B
            controller.Step();
            var bTask = await Task.WhenAny(tcsB.Task, Task.Delay(WaitMs));
            Assert.True(bTask == tcsB.Task, "Node B did not execute on STEP");

            controller.Run();

            // wait for host to finish (poll)
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (host.IsRunning && sw.ElapsedMilliseconds < WaitMs) await Task.Delay(50);

            Assert.False(host.IsRunning, "Host should have completed run");
        }

        [Fact(DisplayName = "StartNode: set when not running, only subgraph executes")]
        public async Task StartNode_Applied_WhenNotRunning_OnlySubgraphRuns()
        {
            // A -> B -> C
            var idA = NodeId.New();
            var idB = NodeId.New();
            var idC = NodeId.New();

            var graph = new GraphModel
            {
                Nodes =
                [
                     new NodeModel
                    {
                        Id = idA,
                        RuntimeType = typeof(FakeRuntimeNode).AssemblyQualifiedName!,
                        Ports =
                        [
                            new PortMetadata
                            {
                                Name = "out",
                                Direction = PortDirection.Output,
                                Kind = PortKind.Data,
                                DataType = new DataTypeId("any"),
                                IsSingle = false
                            }
                        ]
                    },
                    new NodeModel
                    {
                        Id = idB,
                        RuntimeType = typeof(FakeRuntimeNode).AssemblyQualifiedName!,
                        Ports =
                        [
                            new PortMetadata
                            {
                                Name = "in",
                                Direction = PortDirection.Input,
                                Kind = PortKind.Data,
                                DataType = new DataTypeId("any"),
                                IsSingle = false
                            },
                            new PortMetadata
                            {
                                Name = "out",
                                Direction = PortDirection.Output,
                                Kind = PortKind.Data,
                                DataType = new DataTypeId("any"),
                                IsSingle = false
                            }
                        ]
                    },
                    new NodeModel
                    {
                        Id = idC,
                        RuntimeType = typeof(FakeRuntimeNode).AssemblyQualifiedName!,
                        Ports =
                        [
                            new PortMetadata
                            {
                                Name = "in",
                                Direction = PortDirection.Input,
                                Kind = PortKind.Data,
                                DataType = new DataTypeId("any"),
                                IsSingle = false
                            }
                        ]
                    }
                ],
                Links =
                [
                    new LinkModel { FromNode = idA, FromPort = "out", ToNode = idB, ToPort = "in" },
                    new LinkModel { FromNode = idB, FromPort = "out", ToNode = idC, ToPort = "in" }
                ]
            };

            var nodes = new IRuntimeNode[]
            {
            new FakeRuntimeNode(idA, "A", 30),
            new FakeRuntimeNode(idB, "B", 30),
            new FakeRuntimeNode(idC, "C", 30)
            };

            var host = new RuntimeHost(graph, nodes);
            var controller = host.Controller;

            // Apply start node (not running) — should be Applied immediately
            var r = host.TrySetStartNode(idB);
            Assert.Equal(ExecGraph.Runtime.Execution.StartNodeChangeResult.Applied, r);

            // Collect node enter traces
            var entered = new ConcurrentBag<NodeId>();
            var tcsFinished = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            host.Trace.TracePublished += (tr) =>
            {
                if (tr is NodeEnterTrace ne)
                {
                    entered.Add(ne.NodeId);

                    // If both B and C entered, mark finished
                    if (entered.Contains(idB) && entered.Contains(idC))
                    {
                        tcsFinished.TrySetResult(true);
                    }
                }
            };

            // Run automatic to completion
            controller.SetRunMode(ExecGraph.Contracts.Runtime.RunMode.Automatic);
            var rt = new Thread(host.Start) { IsBackground = true };
            rt.Start();

            // Wait for completion or timeout
            var completed = await Task.WhenAny(tcsFinished.Task, Task.Delay(5000));
            Assert.True(completed == tcsFinished.Task, "Run did not finish within timeout");

            // Ensure only B and C executed (A not present)
            Assert.DoesNotContain(idA, entered);
            Assert.Contains(idB, entered);
            Assert.Contains(idC, entered);

            // Wait for host to finish (graceful)
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (host.IsRunning && sw.ElapsedMilliseconds < 2000) await Task.Delay(20);
            Assert.False(host.IsRunning, "Host should have completed run after subgraph execution");
        }

        // Note: we left a placeholder to illustrate checking results; below we'll provide the final implementation for the second test as in the earlier message if you want.
    }
}
