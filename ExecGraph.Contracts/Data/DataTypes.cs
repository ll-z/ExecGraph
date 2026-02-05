
using ExecGraph.Abstractions.Data;

namespace ExecGraph.Contracts.Data
{
    public static class DataTypes
    {
        public static readonly DataTypeId Bool = new("bool");
        public static readonly DataTypeId Float = new("float");
        public static readonly DataTypeId Int = new("int");
        public static readonly DataTypeId UInt = new("uint");
        public static readonly DataTypeId Char = new("char");
        public static readonly DataTypeId String = new("string");
        public static readonly DataTypeId Enum = new("Enum");
        public static readonly DataTypeId DateTime = new("DateTime");
        public static readonly DataTypeId Any = new("any");
    }
}
