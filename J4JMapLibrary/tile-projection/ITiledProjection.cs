using static J4JMapLibrary.TiledProjection;

namespace J4JMapLibrary;

public interface ITiledProjection : IMapProjection
{
    bool CanBeCached { get; }

    int MaxScale { get; }
    int MinScale { get; }
    int Scale { get; set; }

    int TileHeightWidth { get; }

    MapTile MinTile { get; }
    MapTile MaxTile { get; }

    MapPoint CreateMapPoint();

    MapTile? CreateMapTileFromTileCoordinates( int xTile, int yTile );
    MapTile? CreateMapTileFromXY( int x, int y );
    MapTile? CreateMapTileFromCartesian( Cartesian point );
    MapTile? CreateMapTileFromLatLong(LatLong point);

    double GroundResolution( double latitude );
    string MapScale( double latitude, double dotsPerInch );

    LatLong CartesianToLatLong(int x, int y);
    Cartesian LatLongToCartesian(double latitude, double longitude);

    Task<TileImageStream> GetTileImageAsync( MapTile tile );
    IAsyncEnumerable<TileImageStream> GetTileImagesAsync( IEnumerable<MapTile> tiles );
}
