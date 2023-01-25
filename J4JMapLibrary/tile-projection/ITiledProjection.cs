using static J4JMapLibrary.TiledProjection;

namespace J4JMapLibrary;

public interface ITiledProjection : IMapProjection
{
    int MaxScale { get; }
    int MinScale { get; }
    int Scale { get; set; }

    int TileWidth { get; }
    int TileHeight { get; }

    TiledProjection.MapTile MinTile { get; }
    TiledProjection.MapTile MaxTile { get; }

    TiledProjection.MapTile? CreateMapTile( int xTile, int yTile );
    MapTile? CreateMapTileFromXY( int x, int y );

    double GroundResolution( double latitude );
    string MapScale( double latitude, double dotsPerInch );

    Task<TileImageStream> GetTileImageAsync( MapTile tile );
    IAsyncEnumerable<TileImageStream> GetTileImagesAsync( IEnumerable<MapTile> tiles );
}
