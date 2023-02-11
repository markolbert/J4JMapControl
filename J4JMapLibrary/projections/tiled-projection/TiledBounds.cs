namespace J4JMapLibrary;

public class TiledBounds : IEquatable<TiledBounds>
{
    public TiledBounds(
        TileCoordinates upperLeft,
        TileCoordinates lowerRight
    )
    {
        UpperLeft = upperLeft;
        LowerRight = lowerRight;
    }

    public TileCoordinates UpperLeft { get; init; }
    public TileCoordinates LowerRight { get; init; }

    public bool Equals( TiledBounds? other )
    {
        if( ReferenceEquals( null, other ) ) return false;
        if( ReferenceEquals( this, other ) ) return true;

        return UpperLeft.Equals( other.UpperLeft ) && LowerRight.Equals( other.LowerRight );
    }

    public override bool Equals( object? obj )
    {
        if( ReferenceEquals( null, obj ) ) return false;
        if( ReferenceEquals( this, obj ) ) return true;

        return obj.GetType() == GetType() && Equals( (TiledBounds) obj );
    }

    public override int GetHashCode() => HashCode.Combine( UpperLeft, LowerRight );

    public static bool operator==( TiledBounds? left, TiledBounds? right ) => Equals( left, right );

    public static bool operator!=( TiledBounds? left, TiledBounds? right ) => !Equals( left, right );
}
