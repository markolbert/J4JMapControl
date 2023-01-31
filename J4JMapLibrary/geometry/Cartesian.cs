namespace J4JMapLibrary;

public class Cartesian
{
    public EventHandler? Changed;

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

    public int X => _x;
    public int Y => _y;

    public void SetCartesian(int? x, int? y)
    {
        if (x == null && y == null)
            return;

        if( x.HasValue)
            _x = InternalExtensions.ConformValueToRange(x.Value,
                Metrics.YRange,
                "X");

        if ( y.HasValue)
            _y = InternalExtensions.ConformValueToRange(y.Value,
                Metrics.YRange,
                "Y");

        Changed?.Invoke( this, EventArgs.Empty );
    }

    public void SetCartesian(Cartesian cartesian)
    {
            _x = InternalExtensions.ConformValueToRange(cartesian.X,
                Metrics.YRange,
                "X");

            _y = InternalExtensions.ConformValueToRange(cartesian.Y,
                Metrics.YRange,
                "Y");

        Changed?.Invoke(this, EventArgs.Empty);
    }
}