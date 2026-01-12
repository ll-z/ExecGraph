using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Runtime.Execution
{
    public enum StartNodeChangeResult
    {
        Applied,                 // 立即应用（未运行或不危险）
        RequireRestartConfirm,   // 需要弹窗确认并重启
        Pending,                 // 已经作为 pending 存储（编辑器可在 UI 展示）
        Rejected                 // 拒绝（例如参数无效）
    }

    public enum ExecutionResetTrace
    {
        EpochFrom,                 // 立即应用（未运行或不危险）
        EpochTo,   // 需要弹窗确认并重启
        StartNode,                 // 已经作为 pending 存储（编辑器可在 UI 展示）
        Reason                 // 拒绝（例如参数无效）
    }
}
