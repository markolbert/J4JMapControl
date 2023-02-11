namespace J4JMapLibrary;

public class Cartesian
{
    public EventHandler? Changed;

    public Cartesian(
        ITiledScale scale
    )
    {
        XRange = scale.XRange;
        YRange = scale.YRange;
    }

    public Cartesian()
    {
        XRange = new MinMax<int>( int.MinValue, int.MaxValue );
        YRange = new MinMax<int>(int.MinValue, int.MaxValue);
    }

    public MinMax<int> XRange { get; internal set; }
    public int X { get; private set; }

    public MinMax<int> YRange { get; internal set; }
    public int Y { get; private set; }

    public void SetCartesian( int? x, int? y )
    {
        if( x == null && y == null )
            return;

        if( x.HasValue )
            X = XRange.ConformValueToRange( x.Value, "X" );

        if( y.HasValue )
            Y = YRange.ConformValueToRange( y.Value, "Y" );

        Changed?.Invoke( this, EventArgs.Empty );
    }

    public void SetCartesian( Cartesian cartesian )
    {
        X = XRange.ConformValueToRange( cartesian.X, "X" );
        Y = YRange.ConformValueToRange( cartesian.Y, "Y" );

        Changed?.Invoke( this, EventArgs.Empty );
    }
}
