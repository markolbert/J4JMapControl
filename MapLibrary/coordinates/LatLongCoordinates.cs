namespace J4JSoftware.MapLibrary;

public record LatLongCoordinates(
    LatLong LatLongUpperLeft,
    LatLong LatLongLowerRight,
    IMapProjection MapProjection
) : Coordinates( MapProjection )
{
    public virtual bool Equals( LatLongCoordinates? other )
    {
        if( ReferenceEquals( null, other ) )
            return false;
        if( ReferenceEquals( this, other ) )
            return true;

        return base.Equals( other )
         && LatLongUpperLeft.Equals( other.LatLongUpperLeft )
         && LatLongLowerRight.Equals( other.LatLongLowerRight );
    }

    public override int GetHashCode() =>
        HashCode.Combine( base.GetHashCode(), LatLongUpperLeft, LatLongLowerRight );
}
