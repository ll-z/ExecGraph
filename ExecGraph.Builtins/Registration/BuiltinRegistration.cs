using ExecGraph.Contracts.Common;
using ExecGraph.Contracts.Graph;
using ExecGraph.Contracts.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Builtins.Registration
{
    public static class BuiltinRegistration
    {
        /// <summary>
        /// 从当前程序集扫描带 RuntimeNodeAttribute 的类型并注册到 registry。
        /// 约定：节点类型应提供 (NodeId) 或 (NodeId, NodeModel) 的构造函数。
        /// </summary>
        public static void RegisterAll(NodeTypeRegistry registry)
        {
            var asm = Assembly.GetExecutingAssembly();
            foreach (var t in asm.GetTypes())
            {
                if (t.IsAbstract) continue;
                if (!typeof(IRuntimeNode).IsAssignableFrom(t)) continue;

                var attr = t.GetCustomAttribute<RuntimeNodeAttribute>();
                if (attr == null) continue;

                registry.Register(attr.Name ?? t.FullName!, (id, model) =>
                {
                    // 优先使用 (NodeId, NodeModel)
                    var ctor = t.GetConstructor(new[] { typeof(NodeId), typeof(NodeModel) });
                    if (ctor != null) return (IRuntimeNode)ctor.Invoke(new object[] { id, model! });

                    ctor = t.GetConstructor(new[] { typeof(NodeId) });
                    if (ctor != null) return (IRuntimeNode)ctor.Invoke(new object[] { id });

                    // 最后尝试无参构造（并设置 Id，如果需要的话，会有兼容问题）
                    ctor = t.GetConstructor(Type.EmptyTypes);
                    if (ctor != null) return (IRuntimeNode)ctor.Invoke(null);

                    throw new InvalidOperationException($"No compatible constructor for builtin node {t.FullName}");
                });
            }
        }
    }
}
