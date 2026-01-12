using ExecGraph.Contracts.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Contracts.Runtime
{
    public interface IRuntimeContext
    {
        RunMode RunMode { get; }

        T GetInput<T>(string portName);
        void SetOutput<T>(string portName, T value);

        void EmitTrace(TraceEvent trace);
    }
}
