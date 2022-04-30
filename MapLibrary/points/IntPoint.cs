namespace J4JSoftware.MapLibrary;

public record IntPoint( int X, int Y )
{
    public static IntPoint Empty = new( 0, 0 );

    public IntPoint(
        double x,
        double y
    )
        : this( Round( x ), Round( y ) )
    {
    }

    private static int Round( double toRound ) => Convert.ToInt32( Math.Round( toRound ) );
}