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

        result = new LatLong( retrieverInfo ) { Latitude = latitude, Longitude = longitude };

        return true;
    }

    private readonly MapRetrieverInfo _retrieverInfo;
    private double _latitude;
    private double _longitude;

    public LatLong( MapRetrieverInfo retrieverInfo )
    {
        _retrieverInfo = retrieverInfo;
    }

    private LatLong( LatLong toCopy )
        : this( toCopy._retrieverInfo )
    {
        _latitude = toCopy.Latitude;
        _longitude = toCopy.Longitude;
    }

    public LatLong Copy() => new( this );

    public double Latitude
    {
        get => _latitude;

        set
        {
            if( value > MaximumLatitude || value < -MaximumLatitude )
                return;

            _latitude = value;
        }
    }

    public double MaximumLatitude => _retrieverInfo.MaximumLatitude;

    public double Longitude
    {
        get => _longitude;

        set
        {
            if( value > MaximumLongitude || value < -MaximumLongitude )
                return;

            _longitude = value;
        }
    }

    public double MaximumLongitude => _retrieverInfo.MaximumLongitude;

    public override string ToString() => $"{Latitude}, {Longitude}";
}
