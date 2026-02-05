using ExecGraph.Abstractions.Common;


namespace ExecGraph.Contracts.Runtime
{
    public interface IDebugController
    {
        bool IsEnabled { get; }


        void AddBreakpoint(NodeId nodeId);
        void RemoveBreakpoint(NodeId nodeId);
        void ClearBreakpoints();


        bool ShouldBreak(NodeId nodeId);
    }
}
