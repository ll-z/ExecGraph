using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Abstractions.Data
{
    public readonly record struct DataTypeId(string Name)
    {
        public override string ToString() => Name;
    }
}
