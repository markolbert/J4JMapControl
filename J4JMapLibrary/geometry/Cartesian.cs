namespace J4JMapLibrary;

public class Cartesian
{
    public EventHandler? Changed;

    public Cartesian(
        ProjectionMetrics metrics
    )
    {
        XRange = metrics.XRange;
        YRange = metrics.YRange;

        Scale = metrics.Scale;
    }

    public int Scale { get; internal set; }

    public MinMax<int> XRange { get; internal set; }
    public int X { get; private set; }

    public MinMax<int> YRange { get; internal set; }
    public int Y { get; private set; }

    public void SetCartesian(int? x, int? y)
    {
        if (x == null && y == null)
            return;

        if( x.HasValue )
            X = InternalExtensions.ConformValueToRange( x.Value, XRange, "X" );

        if( y.HasValue )
            Y = InternalExtensions.ConformValueToRange( y.Value, YRange, "Y" );

        Changed?.Invoke( this, EventArgs.Empty );
    }

    public void SetCartesian( Cartesian cartesian )
    {
        X = InternalExtensions.ConformValueToRange( cartesian.X, XRange, "X" );
        Y = InternalExtensions.ConformValueToRange( cartesian.Y, YRange, "Y" );

        Changed?.Invoke( this, EventArgs.Empty );
    }
}