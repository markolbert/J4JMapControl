using J4JSoftware.DeusEx;

namespace J4JMapLibrary;

public partial class MapTile
{
    // internal to hopefully avoid stack overflow
    internal MapTile(
        ITiledProjection projection,
        int xTile,
        int yTile,
        byte[] imageData
    )
        : this( projection, xTile, yTile )
    {
        _imageData = imageData;
    }

    private MapTile(
        ITiledProjection projection,
        int xTile,
        int yTile
    )
    {
        _logger = J4JDeusEx.GetLogger<MapTile>()!;
        _createRequest = projection.GetRequest;
        _extractImageStreamAsync = projection.ExtractImageDataAsync;

        HeightWidth = projection.TileHeightWidth;
        Scope = TiledMapScope.Copy( (TiledMapScope) projection.GetScope() );

        MaxRequestLatency = projection.MaxRequestLatency;
        _cancellationTokenSource.CancelAfter( MaxRequestLatency );

        X = xTile < 0 ? 0 : xTile;
        Y = yTile < 0 ? 0 : yTile;
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

        HeightWidth = projection.TileHeightWidth;
        Scope = TiledMapScope.Copy((TiledMapScope)projection.GetScope());

        MaxRequestLatency = projection.MaxRequestLatency;
        _cancellationTokenSource.CancelAfter( MaxRequestLatency );

        X = point.X / projection.TileHeightWidth;
        Y = point.Y / projection.TileHeightWidth;
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

        HeightWidth = projection.TileHeightWidth;
        Scope = TiledMapScope.Copy((TiledMapScope)projection.GetScope());

        var cartesian = Scope.LatLongToCartesian( latLong );
        X = cartesian.X;
        Y = cartesian.Y;

        MaxRequestLatency = projection.MaxRequestLatency;
        _cancellationTokenSource.CancelAfter( MaxRequestLatency );

        QuadKey = this.GetQuadKey();
    }
}
