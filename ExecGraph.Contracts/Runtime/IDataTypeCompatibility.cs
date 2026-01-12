using ExecGraph.Contracts.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Contracts.Runtime
{
    public interface IDataTypeCompatibility
    {
        bool CanAssign(DataTypeId from, DataTypeId to);
    }
}
