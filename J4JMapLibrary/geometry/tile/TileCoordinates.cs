namespace J4JMapLibrary;

public record TileCoordinates(int X, int Y)
{
    private sealed class TileCoordinatesComparer : IEqualityComparer<TileCoordinates>
    {
        public bool Equals( TileCoordinates? x, TileCoordinates? y )
        {
            if( ReferenceEquals( x, y ) )
                return true;
            if( ReferenceEquals( x, null ) )
                return false;
            if( ReferenceEquals( y, null ) )
                return false;
            if( x.GetType() != y.GetType() )
                return false;

            return x.X == y.X
             && x.Y == y.Y;
        }

        public int GetHashCode( TileCoordinates obj )
        {
            return HashCode.Combine( obj.X, obj.Y );
        }
    }

    public static IEqualityComparer<TileCoordinates> DefaultComparer { get; } = new TileCoordinatesComparer();
}
