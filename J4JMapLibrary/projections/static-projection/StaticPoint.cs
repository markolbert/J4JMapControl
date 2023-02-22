namespace J4JSoftware.J4JMapLibrary;

public class StaticPoint
{
    public EventHandler? Changed;

    private bool _suppressUpdate;
    private MinMax<int> _xRange = new(0, 0);
    private MinMax<int> _yRange = new(0, 0);

    public StaticPoint(
        IProjection projection
    )
    {
        Projection = projection;

        projection.MapServer.ScaleChanged += (_, _) => OnScaleChanged();
        OnScaleChangedCartesian();
    }

    public IProjection Projection { get; }

    protected virtual void OnScaleChanged()
    {
        OnScaleChangedCartesian();
    }

    private void OnScaleChangedCartesian()
    {
        _xRange = new MinMax<int>( 0, Projection.MapServer.HeightWidth - 1 );
        _yRange = new MinMax<int>( 0, Projection.MapServer.HeightWidth - 1 );
    }

    public int X { get; private set; }
    public int Y { get; private set; }

    public void SetCartesian( int? x, int? y )
    {
        if( x == null && y == null )
            return;

        if( x.HasValue )
            X = _xRange.ConformValueToRange( x.Value, "X", Projection.MapServer.HeightWidth );

        if( y.HasValue )
            Y = _yRange.ConformValueToRange( y.Value, "Y", Projection.MapServer.HeightWidth );

        UpdateLatLong();
    }

    private void UpdateLatLong()
    {
        if( _suppressUpdate )
            return;

        _suppressUpdate = true;

        SetLatLong((float)(2
                     * Math.Atan(Math.Exp(MapConstants.TwoPi * Y / Projection.MapServer.TileHeightWidth))
                     - MapConstants.HalfPi)
                 / MapConstants.RadiansPerDegree,
                   360 * X / Projection.MapServer.TileHeightWidth - 180);

        _suppressUpdate = false;
        Changed?.Invoke( this, EventArgs.Empty );
    }

    public float Latitude { get; private set; }
    public float Longitude { get; private set; }

    public void SetLatLong( float? latitude, float? longitude )
    {
        if( latitude == null && longitude == null )
            return;

        if( latitude.HasValue )
            Latitude = Projection.MapServer.LatitudeRange.ConformValueToRange( latitude.Value, "Latitude" );

        if( longitude.HasValue )
            Longitude = Projection.MapServer.LongitudeRange.ConformValueToRange( longitude.Value, "Longitude" );

        UpdateCartesian();
    }

    private void UpdateCartesian()
    {
        if (_suppressUpdate)
            return;

        _suppressUpdate = true;

        // x == 0 is the left hand edge of the projection (the x/y origin is in
        // the upper left corner)
        var x = (int) Math.Round( Projection.MapServer.HeightWidth * ( Longitude / 360 + 0.5 ) );

        // another way of calculating Y...leave as comment for testing
        //var latRadians = Latitude * MapConstants.RadiansPerDegree;
        //var sinRatio = ( 1 + Math.Sin( latRadians ) ) / ( 1 - Math.Sin( latRadians ) );
        //var lnLat = Math.Log( sinRatio );
        //var junk = ( 0.5F - lnLat / MapConstants.FourPi ) * Projection.Height;

        // this weird "subtract the calculation from half the height" is due to the
        // fact y values increase going >>down<< the display, so the top is y = 0
        // while the bottom is y = height
        var y = (int) Math.Round( Projection.MapServer.HeightWidth / 2F
                                - Projection.MapServer.HeightWidth
                                * Math.Log( Math.Tan( MapConstants.QuarterPi
                                                    + Latitude * MapConstants.RadiansPerDegree / 2 ) )
                                / MapConstants.TwoPi );

        SetCartesian( x, y );

        _suppressUpdate = false;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
