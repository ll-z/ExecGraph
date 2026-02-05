using ExecGraph.Abstractions.Common;


namespace ExecGraph.Contracts.Runtime
{
    public interface IStartNodeControl
    {
        void SetStartNode(NodeId? startNodeId);
    }
}
