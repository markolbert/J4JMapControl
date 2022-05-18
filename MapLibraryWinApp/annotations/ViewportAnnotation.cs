using System.ComponentModel;
using Windows.Foundation;

namespace J4JSoftware.MapLibrary;

public class ViewportAnnotation : AnnotationBase
{
    public ViewportAnnotation()
        : base( AnnotationType.Viewport )
    {
    }

    public double X { get; set; } = double.NaN;
    public double Y { get; set; } = double.NaN;

    public HorizontalOrigin HorizontalOrigin { get; set; } = HorizontalOrigin.Left;
    public VerticalOrigin VerticalOrigin { get; set; } = VerticalOrigin.Top;

    public override bool Initialize( Size clipSize, IMapProjection mapProjection )
    {
        IsValid = false;

        if( double.IsNaN( X ) || double.IsNaN( Y ) )
            return IsValid;

        IsValid = true;

        var xPoint = HorizontalOrigin switch
        {
            HorizontalOrigin.Left => X,
            HorizontalOrigin.Center => X + clipSize.Width / 2,
            HorizontalOrigin.Right => clipSize.Width - X,
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {typeof( HorizontalOrigin )} value '{HorizontalOrigin}'" )
        };

        var yPoint = VerticalOrigin switch
        {
            VerticalOrigin.Top => Y,
            VerticalOrigin.Middle => Y + clipSize.Height / 2,
            VerticalOrigin.Bottom => clipSize.Height - Y,
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {typeof( VerticalOrigin )} value '{VerticalOrigin}'" )
        };

        Origin = new Point( xPoint, yPoint );

        return IsValid;
    }
}