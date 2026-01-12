using ExecGraph.Contracts.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Contracts.Runtime
{
    public sealed class DefaultDataTypeCompatibility : IDataTypeCompatibility
    {
        public bool CanAssign(DataTypeId from, DataTypeId to)
        {
            if (from == to) return true;
            if (to.Name == "any") return true;

            // 示例：int → float
            if (from.Name == "int" && to.Name == "float")
                return true;

            return false;
        }
    }
}
