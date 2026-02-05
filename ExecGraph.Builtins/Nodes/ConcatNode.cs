// ExecGraph.Builtins/Nodes/ConcatNode.cs
using ExecGraph.Abstractions.Common;
using ExecGraph.Abstractions.Data;
using ExecGraph.Abstractions.Trace;

using ExecGraph.Contracts.Trace;
using ExecGraph.Runtime.Abstractions.Runtime;


namespace ExecGraph.Builtins.Nodes
{
    [RuntimeNode("Concat")]
    public sealed class ConcatNode : IExecutionNode
    {
        public NodeId Id { get; }

        private readonly string _separator;
        private readonly bool _ignoreNulls;
        private readonly bool _trim;
        private readonly bool _ignoreEmptyStrings;

        public ConcatNode(NodeId id, string separator = "", bool ignoreNulls = false, bool trim = false, bool ignoreEmptyStrings = false)
        {
            Id = id;
            _separator = separator ?? string.Empty;
            _ignoreNulls = ignoreNulls;
            _trim = trim;
            _ignoreEmptyStrings = ignoreEmptyStrings;
        }

        public async ValueTask<ExecutionResult> ExecuteAsync(IRuntimeContext ctx)
        {
            var traces = new List<TraceEvent> { new NodeEnterTrace { NodeId = Id } };
            try
            {
                if (ctx.Inputs.TryGetValue("items", out var dvItems) && dvItems.Value is object itemsObj && !(itemsObj is string))
                {
                    var parts = CollectFromObject(itemsObj);
                    var result = Finalize(parts);
                    var outputs = new Dictionary<string, DataValue>
                    {
                        ["result"] = new DataValue(result, dvItems.TypeId)
                    };
                    traces.Add(new NodeLeaveTrace { NodeId = Id });
                    return ExecutionResult.Ok(outputs, traces);
                }

                var allParts = new List<string>();
                foreach (var kv in ctx.Inputs)
                {
                    var dv = kv.Value;
                    if (dv.Value == null)
                    {
                        if (!_ignoreNulls) allParts.Add(string.Empty);
                        continue;
                    }

                    if (dv.Value is string s)
                    {
                        allParts.Add(s);
                        continue;
                    }

                    if (dv.Value is IEnumerable<string> se)
                    {
                        allParts.AddRange(se.Select(x => x ?? string.Empty));
                        continue;
                    }

                    if (dv.Value is System.Collections.IEnumerable e)
                    {
                        foreach (var o in e) allParts.Add(o?.ToString() ?? string.Empty);
                        continue;
                    }

                    allParts.Add(dv.Value.ToString() ?? string.Empty);
                }

                var final = Finalize(allParts);
                var outp = new Dictionary<string, DataValue> { ["result"] = new DataValue(final, new DataTypeId("string")) };

                traces.Add(new NodeLeaveTrace { NodeId = Id });
                return ExecutionResult.Ok(outp, traces);
            }
            catch (Exception ex)
            {
                traces.Add(new NodeErrorTrace { NodeId = Id, ErrorMessage = ex.Message, StackTrace = ex.StackTrace });
                return ExecutionResult.Fail(ex);
            }
        }

        private IEnumerable<string> CollectFromObject(object itemsObj)
        {
            if (itemsObj is IEnumerable<string> se) return se.Select(x => x ?? string.Empty);
            if (itemsObj is System.Collections.IEnumerable e)
            {
                var list = new List<string>();
                foreach (var o in e) list.Add(o?.ToString() ?? string.Empty);
                return list;
            }
            return new[] { itemsObj.ToString() ?? string.Empty };
        }

        private string Finalize(IEnumerable<string> parts)
        {
            var seq = parts.Select(p => _trim ? p?.Trim() ?? string.Empty : p ?? string.Empty);
            if (_ignoreNulls) seq = seq.Where(s => s != null);
            if (_ignoreEmptyStrings) seq = seq.Where(s => !string.IsNullOrEmpty(s));
            return string.Join(_separator, seq);
        }
    }
}
