using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Contracts.Runtime
{
    public interface IExecutionController
    {
        RunMode RunMode { get; }

        /// <summary>
        /// Automatic: continuous token supply
        /// Manual: token only via Step()
        /// </summary>
        void SetRunMode(RunMode mode);
        void Run();
        void Pause();
        void Step();
    }

    
}
