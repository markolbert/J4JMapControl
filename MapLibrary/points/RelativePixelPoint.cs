namespace J4JSoftware.MapLibrary;

public class RelativePixelPoint : PixelPointBase
{
    public static RelativePixelPoint Zero = new();

    public RelativePixelPoint()
        : base( new DoubleLimits(double.MinValue,double.MaxValue) )
    {
    }

    public RelativePixelPoint(
        double x,
        double y
    )
        : this()
    {
        Set( x, y );
    }
}
