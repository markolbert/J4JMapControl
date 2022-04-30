namespace J4JSoftware.MapLibrary;

public record DoublePoint( double X, double Y )
{
    public static DoublePoint Empty = new( 0, 0 );
}
