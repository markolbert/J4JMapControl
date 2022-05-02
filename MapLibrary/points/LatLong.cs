using J4JSoftware.DeusEx;

namespace J4JSoftware.MapLibrary;

public class LatLong
{
    public static LatLong GetEmpty( MapRetrieverInfo info ) => new( info );

    public static bool TryParse( string text, out LatLong? result )
    {
        result = null;

        if( string.IsNullOrEmpty( text ) )
            return false;

        var parts = text.Split( ',' );
        if( parts.Length != 2 )
            return false;

        if( double.TryParse( parts[ 0 ], out var latitude ) )
            return false;

        if( double.TryParse( parts[ 1 ], out var longitude ) )
            return false;

        result = new LatLong();
        result.Set(new GeoPoint(latitude,longitude));

        return true;
    }

    public event EventHandler? ValueChanged;

    private readonly LimitedPoint<double> _latitude;
    private readonly LimitedPoint<double> _longitude;

    public LatLong( MapRetrieverInfo info )
    {
        _latitude = new LimitedPoint<double>( new DoubleLimits( -info.MaximumLatitude, info.MaximumLatitude ) );
        _longitude = new LimitedPoint<double>( new DoubleLimits( -info.MaximumLongitude, info.MaximumLongitude ) );
    }

    private LatLong()
    {
        _latitude = new LimitedPoint<double>(new DoubleLimits(GlobalConstants.Wgs84MaxLatitude, 180));
        _longitude = new LimitedPoint<double>(new DoubleLimits(GlobalConstants.Wgs84MaxLatitude, 180));
    }

    public double Latitude => _latitude.Value;
    public double Longitude => _longitude.Value;

    public void Set( GeoPoint point )
    {
        _latitude.Value = point.Latitude;
        _longitude.Value = point.Longitude;

        OnValueChanged();
    }

    public void Set( LatLong point )
    {
        _latitude.Value = point.Latitude;
        _longitude.Value = point.Longitude;

        OnValueChanged();
    }

    protected virtual void OnValueChanged() => ValueChanged?.Invoke( this, EventArgs.Empty );
}
