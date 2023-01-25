namespace J4JMapLibrary;

public abstract partial class TiledProjection
{
    public record MapTile
    {
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

                return x.TileProjection.Equals( y.TileProjection )
                 && x.X == y.X
                 && x.Y == y.Y;
            }

            public int GetHashCode( MapTile obj )
            {
                return HashCode.Combine( obj.TileProjection, obj.X, obj.Y );
            }
        }

        public static IEqualityComparer<MapTile> DefaultComparer { get; } = new TileProjectionXYEqualityComparer();

        static MapTile()
        {
            CreateMapTileInternal = ( proj, x, y ) => new MapTile( proj, x, y );
        }

        private MapTile(
            ITiledProjection projection,
            int x,
            int y
        )
        {
            TileProjection = projection;

            X = x < 0 ? 0 : x;
            Y = y < 0 ? 0 : y;
        }

        public ITiledProjection TileProjection { get; }
        public int X { get; }
        public int Y { get; }
    }
}
