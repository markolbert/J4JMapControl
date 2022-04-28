namespace J4JSoftware.MapLibrary;

public class LimitedPoint<TValue>
    where TValue : struct, IComparable<TValue>
{
    private readonly IPointValueLimits<TValue> _limits;

    private TValue _value;

    public LimitedPoint(
        IPointValueLimits<TValue> limits
    )
    {
        _limits = limits;
    }

    public TValue Minimum => _limits.Minimum;
    public TValue Maximum => _limits.Maximum;

    public TValue Value
    {
        get =>
            _value.CompareTo( _limits.Minimum ) < 0
                ? _limits.Minimum
                : _value.CompareTo( _limits.Maximum ) > 0
                    ? _limits.Maximum
                    : _value;

        set => _value = value;
    }
}