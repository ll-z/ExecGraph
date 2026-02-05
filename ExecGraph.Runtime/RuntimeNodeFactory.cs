using ExecGraph.Abstractions.Common;
using ExecGraph.Contracts.Graph;
using ExecGraph.Contracts.Runtime;
using ExecGraph.Runtime.Abstractions.Runtime;

namespace ExecGraph.Runtime
{
    public sealed class RuntimeNodeFactory : IRuntimeNodeFactory
    {
        private readonly Dictionary<string, Func<NodeModel, IRuntimeNode>> _registry = new(StringComparer.Ordinal);

        public RuntimeNodeFactory Register(string runtimeType, Func<NodeModel, IRuntimeNode> creator)
        {
            if (string.IsNullOrWhiteSpace(runtimeType))
                throw new ArgumentException("Runtime type must be provided.", nameof(runtimeType));
            _registry[runtimeType] = creator ?? throw new ArgumentNullException(nameof(creator));
            return this;
        }

        public IRuntimeNode Create(NodeModel model)
        {
            if (model is null) throw new ArgumentNullException(nameof(model));

            if (_registry.TryGetValue(model.RuntimeType, out var creator))
                return creator(model);

            var runtimeType = Type.GetType(model.RuntimeType, throwOnError: false);
            if (runtimeType is null)
                throw new InvalidOperationException($"Runtime type '{model.RuntimeType}' could not be resolved for node '{model.Id}'.");

            if (!typeof(IRuntimeNode).IsAssignableFrom(runtimeType))
                throw new InvalidOperationException($"Runtime type '{model.RuntimeType}' does not implement {nameof(IRuntimeNode)}.");

            try
            {
                var ctorWithModel = runtimeType.GetConstructor(new[] { typeof(NodeModel) });
                if (ctorWithModel != null)
                    return (IRuntimeNode)ctorWithModel.Invoke(new object[] { model });

                var ctorWithId = runtimeType.GetConstructor(new[] { typeof(NodeId) });
                if (ctorWithId != null)
                    return (IRuntimeNode)ctorWithId.Invoke(new object[] { model.Id });

                var ctorDefault = runtimeType.GetConstructor(Type.EmptyTypes);
                if (ctorDefault != null)
                    return (IRuntimeNode)Activator.CreateInstance(runtimeType)!;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to construct runtime node for type '{model.RuntimeType}' and node '{model.Id}'.", ex);
            }

            throw new InvalidOperationException($"Runtime type '{model.RuntimeType}' must expose a constructor with NodeModel, NodeId, or no parameters.");
        }
    }
}
