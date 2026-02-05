using ExecGraph.Abstractions.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExecGraph.Runtime.Abstractions.Runtime
{
    /// <summary>
    /// 新的节点接口：返回 ExecutionResult，便于 runtime 统一 commit outputs / traces / error handling。
    /// </summary>
    public interface IExecutionNode
    {
        NodeId Id { get; }
        ValueTask<ExecutionResult> ExecuteAsync(IRuntimeContext ctx);
    }
}
