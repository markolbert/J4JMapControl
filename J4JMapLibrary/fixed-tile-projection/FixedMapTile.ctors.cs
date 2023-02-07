namespace J4JMapLibrary;

public partial class FixedMapTile
{
    // internal to hopefully avoid stack overflow
    internal FixedMapTile(
        IFixedTileProjection projection,
        int xTile,
        int yTile,
        byte[] imageData
    )
        : this(projection, xTile, yTile)
    {
        ImageData = imageData;
    }

    private FixedMapTile(
        IFixedTileProjection projection,
        int xTile,
        int yTile
    ) : base(projection)
    {
        //_createRequest = projection.GetRequest;
        //_extractImageStreamAsync = projection.ExtractImageDataAsync;

        HeightWidth = projection.MapServer.TileHeightWidth;

        X = xTile < 0 ? 0 : xTile;
        Y = yTile < 0 ? 0 : yTile;
        QuadKey = this.GetQuadKey();
    }

    //private FixedMapTile(
    //    IFixedTileProjection projection,
    //    Cartesian point
    //) : base(projection)
    //{
    //    //_createRequest = projection.GetRequest;
    //    //_extractImageStreamAsync = projection.ExtractImageDataAsync;

    //    HeightWidth = projection.TileHeightWidth;

    //    X = point.X / projection.TileHeightWidth;
    //    Y = point.Y / projection.TileHeightWidth;
    //    QuadKey = this.GetQuadKey();
    //}

    //private FixedMapTile(
    //    IFixedTileProjection projection,
    //    LatLong latLong
    //) : base(projection)
    //{
    //    //_createRequest = projection.GetRequest;
    //    //_extractImageStreamAsync = projection.ExtractImageDataAsync;

    //    HeightWidth = projection.TileHeightWidth;

    //    var cartesian = Scope.LatLongToCartesian(latLong);
    //    X = cartesian.X;
    //    Y = cartesian.Y;
    //    QuadKey = this.GetQuadKey();
    //}
}