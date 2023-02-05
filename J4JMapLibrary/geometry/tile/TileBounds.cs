namespace J4JMapLibrary;

public record TileBounds(TileCoordinates UpperLeft, TileCoordinates LowerRight)
{
    private sealed class TileBoundsComparer : IEqualityComparer<TileBounds>
    {
        public bool Equals( TileBounds? x, TileBounds? y )
        {
            if( ReferenceEquals( x, y ) )
                return true;
            if( ReferenceEquals( x, null ) )
                return false;
            if( ReferenceEquals( y, null ) )
                return false;
            if( x.GetType() != y.GetType() )
                return false;

            return x.UpperLeft.Equals( y.UpperLeft )
             && x.LowerRight.Equals( y.LowerRight );
        }

        public int GetHashCode( TileBounds obj )
        {
            return HashCode.Combine( obj.UpperLeft, obj.LowerRight );
        }
    }

    public static IEqualityComparer<TileBounds> DefaultComparer { get; } = new TileBoundsComparer();
}
