using ExecGraph.Contracts.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Contracts.Data
{
    public static class DataTypeCompatibilityRegistry
    {
        public static IDataTypeCompatibility Default { get; set; }= new DefaultDataTypeCompatibility();
    }
}
