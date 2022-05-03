namespace J4JSoftware.MapLibrary;

public record PixelLatLong(
    DoublePoint PixelUpperLeft,
    DoublePoint PixelLowerRight,
    LatLong LatLongUpperLeft,
    LatLong LatLongLowerRight,
    IZoom Zoom
) : Coordinates( PixelUpperLeft, Zoom )
{
    public virtual bool Equals( PixelLatLong? other )
    {
        if( ReferenceEquals( null, other ) )
            return false;
        if( ReferenceEquals( this, other ) )
            return true;

        return base.Equals( other )
         && PixelLowerRight.Equals( other.PixelLowerRight )
         && LatLongUpperLeft.Equals( other.LatLongUpperLeft )
         && LatLongLowerRight.Equals( other.LatLongLowerRight );
    }

    public override int GetHashCode() =>
        HashCode.Combine( base.GetHashCode(), PixelLowerRight, LatLongUpperLeft, LatLongLowerRight );
}
