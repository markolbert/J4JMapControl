using static J4JMapLibrary.TiledProjection;

namespace J4JMapLibrary;

public interface ITiledProjection : IMapProjection
{
    bool CanBeCached { get; }

    int MaxScale { get; }
    int MinScale { get; }
    int Scale { get; set; }

    int TileHeightWidth { get; }

    TiledProjection.MapTile MinTile { get; }
    TiledProjection.MapTile MaxTile { get; }

    TiledProjection.MapPoint CreateMapPoint();

    TiledProjection.MapTile? CreateMapTile( int xTile, int yTile );
    MapTile? CreateMapTileFromXY( int x, int y );

    double GroundResolution( double latitude );
    string MapScale( double latitude, double dotsPerInch );

    TiledProjection.LatLong CartesianToLatLong(int x, int y);
    TiledProjection.Cartesian LatLongToCartesian(double latitude, double longitude);

    Task<TileImageStream> GetTileImageAsync( MapTile tile );
    IAsyncEnumerable<TileImageStream> GetTileImagesAsync( IEnumerable<MapTile> tiles );
}
