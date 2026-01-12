using ExecGraph.Contracts.Common;
using ExecGraph.Contracts.Graph;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Runtime.VM
{
    //运行时数据中心
    internal sealed class DataStore
    {
        // (NodeId, PortName) -> value
        private readonly ConcurrentDictionary<(NodeId, string), object?> _values = new();


        // Graph 拓扑
        private readonly Dictionary<(NodeId, string), List<(NodeId, string)>> _routes = new();


        public DataStore(GraphModel graph)
        {
            // 构建数据路由表（Output → Inputs）
            if (graph?.Links == null) return;
            foreach (var link in graph.Links)
            {
                var from = (link.FromNode, link.FromPort);
                var to = (link.ToNode, link.ToPort);


                if (!_routes.TryGetValue(from, out var list))
                {
                    list = new List<(NodeId, string)>();
                    _routes[from] = list;
                }
                list.Add(to);
            }
        }


        public T GetInput<T>(NodeId nodeId, string port)
        {
            var key = (nodeId, port);
            return _values.TryGetValue(key, out var v) ? (T)v! : default!;
        }


        public void SetOutput<T>(NodeId nodeId, string port, T value)
        {
            var from = (nodeId, port);


            if (!_routes.TryGetValue(from, out var targets))
                return;


            foreach (var (toNode, toPort) in targets)
            {
                _values[(toNode, toPort)] = value;
            }
        }
    }
}
