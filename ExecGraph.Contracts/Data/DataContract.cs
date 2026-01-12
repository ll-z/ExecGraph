using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Contracts.Data
{
    public sealed class DataContract
    {
        public required DataTypeId Type { get; init; }
        public object? Value { get; init; }
    }
}
