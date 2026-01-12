using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Runtime
{
    /// <summary>
    /// High-level execution state managed by RuntimeHost.
    ///
    /// This state is orthogonal to RunMode.
    /// </summary>
    public enum ExecutionState
    {
        /// <summary>
        /// No execution in progress.
        /// Execution may start or execution plan may be modified.
        /// </summary>
        Idle,

        /// <summary>
        /// Scheduler is actively consuming execution tokens.
        /// </summary>
        Executing,

        /// <summary>
        /// A manual execution jump has been requested
        /// and awaits explicit confirmation or cancellation.
        ///
        /// Execution is strictly forbidden in this state.
        /// </summary>
        PendingRestart
    }
}
