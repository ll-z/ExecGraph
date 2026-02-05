using ExecGraph.Abstractions.Data;


namespace ExecGraph.Contracts.Data
{
    public sealed class DataContract
    {
        public required DataTypeId Type { get; init; }
        public object? Value { get; init; }
    }
}
