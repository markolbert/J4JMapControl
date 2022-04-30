namespace J4JSoftware.MapLibrary;

public interface IPointValueLimits<out TValue>
    where TValue : struct, IComparable<TValue>
{
    TValue Minimum { get; }
    TValue Maximum { get; }
}
