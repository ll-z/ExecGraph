using ExecGraph.Abstractions.Common;
using ExecGraph.Contracts.Graph;
using ExecGraph.Contracts.Runtime;


namespace ExecGraph.Runtime.Validation
{
    public sealed class GraphValidationResult
    {
        public GraphValidationResult(IReadOnlyList<string> errors)
        {
            Errors = errors;
        }

        public IReadOnlyList<string> Errors { get; }
        public bool IsValid => Errors.Count == 0;
    }

    public sealed class GraphValidator
    {
        public GraphValidationResult Validate(GraphModel graph, IDataTypeCompatibility compatibility)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            if (compatibility == null) throw new ArgumentNullException(nameof(compatibility));

            var errors = new List<string>();
            var nodes = graph.Nodes.ToDictionary(node => node.Id);

            var connectionCounts = new Dictionary<(NodeId Id, string Port, PortDirection Direction), int>();

            if (graph.Links != null)
            {
                foreach (var link in graph.Links)
                {
                    if (!nodes.TryGetValue(link.FromNode, out var fromNode))
                    {
                        errors.Add($"Missing source node '{link.FromNode}'.");
                        continue;
                    }

                    if (!nodes.TryGetValue(link.ToNode, out var toNode))
                    {
                        errors.Add($"Missing target node '{link.ToNode}'.");
                        continue;
                    }

                    var fromPort = fromNode.Ports.FirstOrDefault(port => port.Name == link.FromPort);
                    if (fromPort == null)
                    {
                        errors.Add($"Missing source port '{link.FromPort}' on node '{fromNode.Id}'.");
                        continue;
                    }

                    var toPort = toNode.Ports.FirstOrDefault(port => port.Name == link.ToPort);
                    if (toPort == null)
                    {
                        errors.Add($"Missing target port '{link.ToPort}' on node '{toNode.Id}'.");
                        continue;
                    }

                    if (fromPort.Direction != PortDirection.Output)
                        errors.Add($"Port '{fromNode.Id}.{fromPort.Name}' must be Output for link to '{toNode.Id}.{toPort.Name}'.");

                    if (toPort.Direction != PortDirection.Input)
                        errors.Add($"Port '{toNode.Id}.{toPort.Name}' must be Input for link from '{fromNode.Id}.{fromPort.Name}'.");

                    if (!compatibility.CanAssign(fromPort.DataType, toPort.DataType))
                    {
                        errors.Add($"Incompatible data types: '{fromNode.Id}.{fromPort.Name}' ({fromPort.DataType}) -> '{toNode.Id}.{toPort.Name}' ({toPort.DataType}).");
                    }

                    IncrementCount(connectionCounts, (fromNode.Id, fromPort.Name, fromPort.Direction));
                    IncrementCount(connectionCounts, (toNode.Id, toPort.Name, toPort.Direction));
                }
            }

            foreach (var node in graph.Nodes)
            {
                foreach (var port in node.Ports)
                {
                    if (!port.IsSingle) continue;
                    connectionCounts.TryGetValue((node.Id, port.Name, port.Direction), out var count);
                    if (count > 1)
                    {
                        errors.Add($"Port '{node.Id}.{port.Name}' allows a single connection but has {count}.");
                    }
                }
            }

            return new GraphValidationResult(errors);
        }

        public void ValidateOrThrow(GraphModel graph, IDataTypeCompatibility compatibility)
        {
            var result = Validate(graph, compatibility);
            if (result.IsValid) return;

            var message = $"Graph validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, result.Errors)}";
            throw new InvalidOperationException(message);
        }

        private static void IncrementCount(Dictionary<(NodeId Id, string Port, PortDirection Direction), int> counts,
            (NodeId Id, string Port, PortDirection Direction) key)
        {
            if (counts.TryGetValue(key, out var existing))
                counts[key] = existing + 1;
            else
                counts[key] = 1;
        }
    }
}
