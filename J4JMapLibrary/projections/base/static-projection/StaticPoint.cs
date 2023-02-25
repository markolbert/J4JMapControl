namespace J4JSoftware.J4JMapLibrary;

public class StaticPoint
{
    public EventHandler? Changed;

    private bool _suppressUpdate;
    private int _scale;

    public StaticPoint(
        IProjection projection
    )
    {
        Projection = projection;
    }

    public IProjection Projection { get; }

    public int Scale
    {
        get => _scale;

        set
        {
            _scale = Projection.ScaleRange.ConformValueToRange( value, $"{GetType().Name} Scale" );

            UpdateCartesian();
        }
    }

    public int X { get; private set; }
    public int Y { get; private set; }

    public void SetCartesian( int? x, int? y )
    {
        if( x == null && y == null )
            return;

        if( x.HasValue )
        {
            var xRange = Projection.GetXRange( Scale );
            X = xRange.ConformValueToRange( x.Value, $"{GetType().Name} X" );
        }

        if( y.HasValue )
        {
            var yRange = Projection.GetYRange( Scale );
            Y = yRange.ConformValueToRange( y.Value, $"{GetType().Name} X" );
        }

        UpdateLatLong();
    }

    private void UpdateLatLong()
    {
        if( _suppressUpdate )
            return;

        _suppressUpdate = true;

        var heightWidth = (double) Projection.GetHeightWidth( Scale );

        var scaledX = X / heightWidth - 0.5;
        var scaledY = 0.5 - Y / heightWidth;
        var latitude = (float) ( 90 - 360 * Math.Atan( Math.Exp( -scaledY * MapConstants.TwoPi ) ) / Math.PI );
        var longitude = (float) ( 360 * scaledX );

        SetLatLong( latitude, longitude );

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
            Latitude = Projection.LatitudeRange.ConformValueToRange( latitude.Value, "Latitude" );

        if( longitude.HasValue )
            Longitude = Projection.LongitudeRange.ConformValueToRange( longitude.Value, "Longitude" );

        UpdateCartesian();
    }

    private void UpdateCartesian()
    {
        if (_suppressUpdate)
            return;

        _suppressUpdate = true;

        var heightWidth = Projection.GetHeightWidth(Scale);

        // x == 0 is the left hand edge of the projection (the x/y origin is in
        // the upper left corner)
        var x = (int) Math.Round( heightWidth * ( Longitude / 360 + 0.5 ) );

        // another way of calculating Y...leave as comment for testing
        //var latRadians = Latitude * MapConstants.RadiansPerDegree;
        //var sinRatio = ( 1 + Math.Sin( latRadians ) ) / ( 1 - Math.Sin( latRadians ) );
        //var lnLat = Math.Log( sinRatio );
        //var junk = ( 0.5F - lnLat / MapConstants.FourPi ) * Projection.Height;

        // this weird "subtract the calculation from half the height" is due to the
        // fact y values increase going >>down<< the display, so the top is y = 0
        // while the bottom is y = height
        var y = (int) Math.Round( heightWidth / 2F
                                - heightWidth
                                * Math.Log( Math.Tan( MapConstants.QuarterPi
                                                    + Latitude * MapConstants.RadiansPerDegree / 2 ) )
                                / MapConstants.TwoPi );

        SetCartesian( x, y );

        _suppressUpdate = false;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
