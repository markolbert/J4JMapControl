namespace J4JMapLibrary;

public partial class MapTile
{
    #region IEqualityComparer

    private sealed class TileProjectionXYEqualityComparer : IEqualityComparer<MapTile>
    {
        public bool Equals( MapTile? x, MapTile? y )
        {
            if( ReferenceEquals( x, y ) )
                return true;
            if( ReferenceEquals( x, null ) )
                return false;
            if( ReferenceEquals( y, null ) )
                return false;
            if( x.GetType() != y.GetType() )
                return false;

            return x.Metrics.Equals( y.Metrics )
             && x.X == y.X
             && x.Y == y.Y;
        }

        public int GetHashCode( MapTile obj )
        {
            return HashCode.Combine( obj.Metrics, obj.X, obj.Y );
        }
    }

    public static IEqualityComparer<MapTile> DefaultComparer { get; } = new TileProjectionXYEqualityComparer();

    #endregion
}
