
namespace ExecGraph.Abstractions.Data
{
    /// <summary>
    /// 不可变的数据单元，包含数据 + 类型 + 可选元数据。
    /// 保留原有 public API，同时增加 TryGet/As 便捷方法，便于测试与类型检查。
    /// </summary>
    public sealed record DataValue(object? Value, DataTypeId TypeId, IReadOnlyDictionary<string, object?>? Meta = null)
    {
        public bool TryGet<T>(out T? value)
        {
            if (Value is T t)
            {
                value = t;
                return true;
            }
            // 若需要，可以在此处加入常见的隐式转换/解析逻辑（string->int 等），但默认不做隐式转换。
            value = default;
            return false;
        }

        public T? As<T>() => TryGet<T>(out var v) ? v : default;
    }
}
