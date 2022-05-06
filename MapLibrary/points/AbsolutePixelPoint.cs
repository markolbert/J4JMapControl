namespace J4JSoftware.MapLibrary;

public class AbsolutePixelPoint : PixelPointBase
{
    public static AbsolutePixelPoint Zero = new();

    public AbsolutePixelPoint()
        : base( new DoubleLimits( 0, double.MaxValue ) )
    {
    }

    public AbsolutePixelPoint( double x, double y )
        : this()
    {
        Set( x, y );
    }
}
