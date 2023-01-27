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

    double GroundResolution( double latitude );
    string MapScale( double latitude, double dotsPerInch );

    LatLong CartesianToLatLong(int x, int y);
    Cartesian LatLongToCartesian(double latitude, double longitude);

    bool TryGetRequest( MapTile tile, out HttpRequestMessage? result );
    Task<MemoryStream?> ExtractImageDataAsync( HttpResponseMessage response );
}
