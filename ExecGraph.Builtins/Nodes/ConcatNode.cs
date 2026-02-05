using ExecGraph.Contracts.Common;
using ExecGraph.Contracts.Data;
using ExecGraph.Contracts.Runtime;
using ExecGraph.Contracts.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExecGraph.Builtins.Nodes
{
    [RuntimeNode("Concat")]
    public sealed class ConcatNode : IRuntimeNode
    {
        public NodeId Id { get; }

        private readonly string _separator;
        private readonly bool _ignoreNulls;
        private readonly bool _trim;
        private readonly bool _ignoreEmptyStrings;

        // 从 NodeModel 创建的 factory 可以传入这些配置
        public ConcatNode(NodeId id, string separator = "", bool ignoreNulls = false, bool trim = false, bool ignoreEmptyStrings = false)
        {
            Id = id;
            _separator = separator ?? string.Empty;
            _ignoreNulls = ignoreNulls;
            _trim = trim;
            _ignoreEmptyStrings = ignoreEmptyStrings;
        }

        public async ValueTask ExecuteAsync(IRuntimeContext ctx)
        {
            // 进入 trace
            ctx.EmitTrace(new NodeEnterTrace { NodeId = Id });

            // 首先优先 items
            if (ctx.Inputs.TryGetValue("items", out var dvItems) && dvItems.Value is object itemsObj && !(itemsObj is string))
            {
                var parts = CollectFromObject(itemsObj);
                var result = Finalize(parts);
                await ctx.SetOutputAsync("result", new DataValue(result, dvItems.TypeId));
                ctx.EmitTrace(new NodeLeaveTrace { NodeId = Id });
                return;
            }

            // 否则遍历所有 Inputs，按字典顺序（runtime 保证顺序或 NodeModel 中保证）
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

                // 如果是 string enumerable
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
            await ctx.SetOutputAsync("result", new DataValue(final, new DataTypeId("string")));
            ctx.EmitTrace(new NodeLeaveTrace { NodeId = Id });
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
