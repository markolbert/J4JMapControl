namespace J4JSoftware.MapLibrary;

public record GeoPoint( double Latitude, double Longitude )
{
    public static GeoPoint Empty = new( 0, 0 );

    public GeoPoint(
        LatLong latLong
    )
        : this( latLong.Latitude, latLong.Longitude )
    {
    }
}