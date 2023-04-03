using System.Numerics;

namespace J4JSoftware.J4JMapLibrary;

public class MapPoint
{
    public EventHandler? Changed;

    private bool _suppressUpdate;

    public MapPoint(
        MapRegion.MapRegion region
    )
    {
        Region = region;
    }

    public MapPoint Copy()
    {
        var retVal = (MapPoint) MemberwiseClone();
        retVal.Changed = null;
        retVal.Region = Region;

        return retVal;
    }

    public MapRegion.MapRegion Region { get; private set; }

    public float X { get; private set; }
    public float Y { get; private set; }

    public void SetCartesian( float? x, float? y )
    {
        if( x == null && y == null )
            return;

        if( x.HasValue )
        {
            var xRange = Region.Projection.GetXYRange( Region.Scale );
            X = xRange.ConformValueToRange( x.Value, $"{GetType().Name} X" );
        }

        if( y.HasValue )
        {
            var yRange = Region.Projection.GetXYRange( Region.Scale );
            Y = yRange.ConformValueToRange( y.Value, $"{GetType().Name} Y" );
        }

        UpdateLatLong();
    }

    public void OffsetCartesian( float? xOffset, float? yOffset )
    {
        if( xOffset == null && yOffset == null )
            return;

        var x = X + xOffset;
        var y = Y + yOffset;

        SetCartesian( x, y );
    }

    //public void Rotate( float angle )
    //{
    //    angle = angle % 360;

    //    if( angle == 0 )
    //        return;

    //    var curPoint = new Vector3( X, Y, 0 );

    //    var heightWidth = (float) Region.Projection.GetHeightWidth(Region.Scale);
    //    var centerPoint = new Vector3( heightWidth / 2, heightWidth / 2, 0 );

    //    var transform = Matrix4x4.CreateRotationZ( angle * MapConstants.RadiansPerDegree, centerPoint );
    //    var rotatedPoint = Vector3.Transform( curPoint, transform );

    //    SetCartesian( rotatedPoint.X, rotatedPoint.Y );
    //}

    private void UpdateLatLong()
    {
        if( _suppressUpdate )
            return;

        _suppressUpdate = true;

        var heightWidth = (double) Region.Projection.GetHeightWidth( Region.Scale );

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
            Latitude = Region.Projection.LatitudeRange.ConformValueToRange( latitude.Value, "Latitude" );

        if( longitude.HasValue )
            Longitude = Region.Projection.LongitudeRange.ConformValueToRange( longitude.Value, "Longitude" );

        UpdateCartesian();
    }

    private void UpdateCartesian()
    {
        if( _suppressUpdate )
            return;

        _suppressUpdate = true;

        var heightWidth = Region.Projection.GetHeightWidth( Region.Scale );

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
        Changed?.Invoke( this, EventArgs.Empty );
    }
}
