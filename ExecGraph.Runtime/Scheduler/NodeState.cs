using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Runtime.Scheduler
{
    public enum NodeState
    {
        Idle,
        Ready,
        Executing,
        Completed
    }
}
