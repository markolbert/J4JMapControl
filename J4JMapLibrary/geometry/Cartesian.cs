using J4JSoftware.Logging;

namespace J4JMapLibrary;

public class Cartesian
{
    private int _x;
    private int _y;

    public Cartesian(
        ProjectionMetrics metrics
    )
    {
        Metrics = metrics;
        Scale = metrics.Scale;
    }

    public ProjectionMetrics Metrics { get; }
    public int Scale { get; }
    public bool ReflectsProjection => Scale == Metrics.Scale;

    public int X
    {
        get => _x;

        set =>
            _x = InternalExtensions.ConformValueToRange( value,
                                                         Metrics.XRange,
                                                         "X" );
    }

    public int Y
    {
        get => _y;

        set =>
            _y =
                InternalExtensions.ConformValueToRange( value,
                                                        Metrics.YRange,
                                                        "Y" );
    }
}