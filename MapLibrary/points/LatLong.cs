using J4JSoftware.DeusEx;

namespace J4JSoftware.MapLibrary;

public class LatLong
{
    public static LatLong GetEmpty( MapRetrieverInfo info ) => new( info );

    public static bool TryParse( string text, MapRetrieverInfo retrieverInfo, out LatLong? result )
    {
        result = null;

        if( string.IsNullOrEmpty( text ) )
            return false;

        var parts = text.Split( ',' );
        if( parts.Length != 2 )
            return false;

        if( !double.TryParse( parts[ 0 ], out var latitude ) )
            return false;

        if( !double.TryParse( parts[ 1 ], out var longitude ) )
            return false;

        result = new LatLong( retrieverInfo );
        result.Set( latitude, longitude );

        return true;
    }

    public event EventHandler? ValueChanged;

    private readonly LimitedPoint<double> _latitude;
    private readonly LimitedPoint<double> _longitude;
    private readonly MapRetrieverInfo _retrieverInfo;

    public LatLong( MapRetrieverInfo retrieverInfo )
    {
        _retrieverInfo = retrieverInfo;

        _latitude = new LimitedPoint<double>( new DoubleLimits( -_retrieverInfo.MaximumLatitude,
                                                                _retrieverInfo.MaximumLatitude ) );

        _longitude =
            new LimitedPoint<double>( new DoubleLimits( -_retrieverInfo.MaximumLongitude,
                                                        _retrieverInfo.MaximumLongitude ) );
    }

    private LatLong( LatLong toCopy )
        : this( toCopy._retrieverInfo )
    {
        _latitude.Value = toCopy.Latitude;
        _longitude.Value = toCopy.Longitude;
    }

    public LatLong Copy() => new( this );

    public double Latitude => _latitude.Value;
    public double Longitude => _longitude.Value;

    public void Set( double latitude, double longitude )
    {
        _latitude.Value = latitude;
        _longitude.Value = longitude;

        OnValueChanged();
    }

    public void Set( LatLong point )
    {
        _latitude.Value = point.Latitude;
        _longitude.Value = point.Longitude;

        OnValueChanged();
    }

    protected virtual void OnValueChanged() => ValueChanged?.Invoke( this, EventArgs.Empty );

    public override string ToString() => $"{Latitude}, {Longitude}";
}
