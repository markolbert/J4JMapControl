namespace J4JSoftware.MapLibrary;

public record TilePoint( int X, int Y )
{
    public TilePoint(
        double x,
        double y
    )
        : this( Convert.ToInt32( Math.Floor( x ) ), Convert.ToInt32( Math.Floor( y ) ) )
    {
    }
}