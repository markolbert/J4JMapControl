namespace J4JSoftware.MapLibrary;

public record DoublePoint( double Latitude, double Longitude )
{
    public static DoublePoint Empty = new( 0, 0 );

    public DoublePoint(
        LatLong latLong
    )
        : this( latLong.Latitude, latLong.Longitude )
    {
    }
}
