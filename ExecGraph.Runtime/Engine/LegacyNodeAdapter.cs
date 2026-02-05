// ExecGraph.Runtime/Engine/LegacyNodeAdapter.cs
using ExecGraph.Abstractions.Data;
using ExecGraph.Abstractions.Trace;
using ExecGraph.Contracts.Runtime;
using ExecGraph.Runtime.Abstractions.Runtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExecGraph.Runtime.Engine
{
    /// <summary>
    /// 将旧式的 IRuntimeNode 包装成返回 ExecutionResult 的执行器。
    /// </summary>
    internal static class LegacyNodeAdapter
    {
        public static async ValueTask<ExecutionResult> ExecuteLegacyAsync(IRuntimeNode legacyNode, IRuntimeContext runtimeCtx)
        {
            var capture = new CapturingRuntimeContext(runtimeCtx);
            try
            {
                // 调用旧节点实现（旧节点会调用 capture.SetOutputAsync() / EmitTrace()）
                await legacyNode.ExecuteAsync(capture);

                // 把捕获的 outputs / traces 转成 ExecutionResult
                var outputs = new Dictionary<string, DataValue>(capture.CapturedOutputs);
                var traces = new List<TraceEvent>(capture.CapturedTraces);

                return ExecutionResult.Ok(outputs, traces);
            }
            catch (Exception ex)
            {
                return ExecutionResult.Fail(ex);
            }
        }
    }
}
