using System.Drawing;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract partial class TiledProjection
{
    public record MapTile
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

                return x.Projection.Equals( y.Projection )
                 && x.X == y.X
                 && x.Y == y.Y;
            }

            public int GetHashCode( MapTile obj )
            {
                return HashCode.Combine( obj.Projection, obj.X, obj.Y );
            }
        }

        public static IEqualityComparer<MapTile> DefaultComparer { get; } = new TileProjectionXYEqualityComparer();

        #endregion

        static MapTile()
        {
            MapTileFromCoordinates = ( proj, x, y, logger ) => new MapTile( proj, x, y, logger );
            MapTileFromMapPoint = ( pt, logger ) => new MapTile( pt, logger );
            MapTileFromLatLong = ( latLong, logger ) => new MapTile( latLong, logger );
        }

        public event EventHandler? ImageChanged;

        private readonly IJ4JLogger _logger;
        private MemoryStream? _imageStream;

        private MapTile(
            ITiledProjection projection,
            int x,
            int y,
            IJ4JLogger logger
        )
        {
            _logger = logger;
            _logger.SetLoggedType(GetType());

            Projection = projection;
            Scale = projection.Scale;

            var center = CreateMapPointInternal( projection, _logger );
            center.Cartesian.X = x * Projection.TileHeightWidth + Projection.TileHeightWidth / 2;
            center.Cartesian.Y = y * Projection.TileHeightWidth + Projection.TileHeightWidth / 2;
            Center = center;

            X = x < 0 ? 0 : x;
            Y = y < 0 ? 0 : y;
            QuadKey = this.GetQuadKey();
        }

        private MapTile(
            MapPoint center,
            IJ4JLogger logger
        )
        {
            _logger = logger;
            _logger.SetLoggedType(GetType());

            Projection = center.Projection;
            Scale = center.Projection.Scale;

            Center = center;
            QuadKey = this.GetQuadKey();
        }

        private MapTile(
            LatLong latLong,
            IJ4JLogger logger
        )
        {
            _logger = logger;
            _logger.SetLoggedType( GetType() );

            Projection = latLong.Projection;
            Scale = latLong.Projection.Scale;

            var center = CreateMapPointInternal( Projection, _logger );
            center.LatLong.Latitude = latLong.Latitude;
            center.LatLong.Longitude = latLong.Longitude;
            Center = center;

            QuadKey = this.GetQuadKey();
        }

        public ITiledProjection Projection { get; }
        public int Scale { get; }
        public bool ReflectsProjection => Scale == Projection.Scale;
        public bool CanBeCached => Projection.CanBeCached;

        public MapPoint Center { get; }
        public int HeightWidth => Projection.TileHeightWidth;
        public string QuadKey { get; }
        public int X { get; }
        public int Y { get; }

        public MemoryStream? ImageStream
        {
            get => _imageStream;

            set
            {
                _imageStream = value;
                ImageChanged?.Invoke( this, EventArgs.Empty );
            }
        }
    }
}
