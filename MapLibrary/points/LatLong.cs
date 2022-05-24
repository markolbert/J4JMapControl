namespace J4JSoftware.MapLibrary;

public class LatLong
{
    public static LatLong Empty { get; } = new LatLong();

    public static bool TryParse( string text, out LatLong? result )
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

        result = new LatLong { Latitude = latitude, Longitude = longitude };

        return true;
    }

    public LatLong()
    {
    }

    private LatLong( LatLong toCopy )
    {
        Latitude = toCopy.Latitude;
        Longitude = toCopy.Longitude;
    }

    public LatLong Copy() => new( this );

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public override string ToString() => $"{Latitude}, {Longitude}";
}
