namespace J4JMapLibrary;

public abstract partial class TiledProjection
{
    public record TileImageStream
    {
        static TileImageStream()
        {
            CreateTileImageStream = ( proj, tile, memStream ) => new TileImageStream( proj, tile, memStream );
        }

        private TileImageStream(
            ITiledProjection projection,
            MapTile tile,
            MemoryStream? stream = null
        )
        {
            Projection = projection;
            Tile = tile;
            ImageStream = stream;
        }

        public ITiledProjection Projection { get; }
        public MapTile Tile { get; }
        public MemoryStream? ImageStream { get; }
        public bool IsValid => ImageStream != null;
    }
}
