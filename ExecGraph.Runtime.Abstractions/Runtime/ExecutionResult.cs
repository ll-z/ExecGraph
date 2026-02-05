using ExecGraph.Abstractions.Data;
using ExecGraph.Abstractions.Trace;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExecGraph.Runtime.Abstractions.Runtime
{
    /// <summary>
    /// 执行结果：包含 outputs / traces / error 信息。
    /// </summary>
    public sealed record ExecutionResult
    {
        public bool Success { get; init; }
        public Exception? Error { get; init; }
        public IReadOnlyDictionary<string, DataValue>? Outputs { get; init; }
        public IReadOnlyList<TraceEvent>? Traces { get; init; }

        public static ExecutionResult Ok(IReadOnlyDictionary<string, DataValue>? outputs = null, IReadOnlyList<TraceEvent>? traces = null)
            => new ExecutionResult { Success = true, Outputs = outputs, Traces = traces };

        public static ExecutionResult Fail(Exception ex)
            => new ExecutionResult { Success = false, Error = ex };
    }
}
