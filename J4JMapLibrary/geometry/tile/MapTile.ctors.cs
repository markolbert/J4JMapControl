using J4JSoftware.DeusEx;

namespace J4JMapLibrary;

public partial class MapTile
{
    // internal to hopefully avoid stack overflow
    internal MapTile(
        ITiledProjection projection,
        int x,
        int y,
        byte[] imageData
    )
        : this( projection, x, y )
    {
        _imageData = imageData;
    }

    private MapTile(
        ITiledProjection projection,
        int x,
        int y
    )
    {
        _logger = J4JDeusEx.GetLogger<MapTile>()!;
        _createRequest = projection.GetRequest;
        _extractImageStreamAsync = projection.ExtractImageDataAsync;

        Metrics = projection.Metrics;
        Scale = projection.Scale;
        MaxRequestLatency = projection.MaxRequestLatency;
        HeightWidth = projection.TileHeightWidth;

        Center = new MapPoint( Metrics )
        {
            Cartesian =
            {
                X = x * projection.TileHeightWidth + projection.TileHeightWidth / 2,
                Y = y * projection.TileHeightWidth + projection.TileHeightWidth / 2
            }
        };

        X = x < 0 ? 0 : x;
        Y = y < 0 ? 0 : y;
        QuadKey = this.GetQuadKey();
    }

    private MapTile(
        ITiledProjection projection,
        Cartesian point
    )
    {
        _logger = J4JDeusEx.GetLogger<MapTile>()!;
        _createRequest = projection.GetRequest;
        _extractImageStreamAsync = projection.ExtractImageDataAsync;

        Metrics = projection.Metrics;
        Scale = projection.Scale;
        MaxRequestLatency = projection.MaxRequestLatency;
        HeightWidth = projection.TileHeightWidth;

        Center = new MapPoint( Metrics ) { Cartesian = { X = point.X, Y = point.Y } };

        X = point.X / projection.TileHeightWidth;
        Y = point.Y / projection.TileHeightWidth;
        QuadKey = this.GetQuadKey();
    }

    private MapTile(
        ITiledProjection projection,
        MapPoint center
    )
    {
        _logger = J4JDeusEx.GetLogger<MapTile>()!;
        _createRequest = projection.GetRequest;
        _extractImageStreamAsync = projection.ExtractImageDataAsync;

        Metrics = projection.Metrics;
        Scale = projection.Scale;
        MaxRequestLatency = projection.MaxRequestLatency;
        HeightWidth = projection.TileHeightWidth;

        Center = center;
        QuadKey = this.GetQuadKey();
    }

    private MapTile(
        ITiledProjection projection,
        LatLong latLong
    )
    {
        _logger = J4JDeusEx.GetLogger<MapTile>()!;
        _createRequest = projection.GetRequest;
        _extractImageStreamAsync = projection.ExtractImageDataAsync;

        Metrics = projection.Metrics;
        Scale = projection.Scale;
        MaxRequestLatency = projection.MaxRequestLatency;
        HeightWidth = projection.TileHeightWidth;

        Center = new MapPoint( Metrics ) { LatLong = { Latitude = latLong.Latitude, Longitude = latLong.Longitude } };

        QuadKey = this.GetQuadKey();
    }
}
