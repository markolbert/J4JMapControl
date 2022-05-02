namespace J4JSoftware.MapLibrary;

public record ScreenGlobalCoordinates(
    DoublePoint ScreenUpperLeft,
    DoublePoint ScreenLowerRight,
    LatLong UpperLeft,
    LatLong LowerRight,
    IZoom Zoom
) : Coordinates( ScreenUpperLeft, Zoom )
{
    public virtual bool Equals( ScreenGlobalCoordinates? other )
    {
        if( ReferenceEquals( null, other ) )
            return false;
        if( ReferenceEquals( this, other ) )
            return true;

        return base.Equals( other )
         && ScreenLowerRight.Equals( other.ScreenLowerRight )
         && UpperLeft.Equals( other.UpperLeft )
         && LowerRight.Equals( other.LowerRight );
    }

    public override int GetHashCode() =>
        HashCode.Combine( base.GetHashCode(), ScreenLowerRight, UpperLeft, LowerRight );
}
