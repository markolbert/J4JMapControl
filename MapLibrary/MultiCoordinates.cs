namespace J4JSoftware.MapLibrary;

public record MultiCoordinates
{
    public MultiCoordinates(
        TilePoint tilePoint,
        IMapProjection mapProjection,
        CoordinateOrigin origin
    )
    {
        TilePoint = tilePoint;

        ScreenPoint = new DoublePoint( tilePoint.X * mapProjection.TileWidthHeight,
                                       tilePoint.Y * mapProjection.TileWidthHeight,
                                       origin,
                                       mapProjection );

        LatLong = new LatLong( mapProjection.MapRetrieverInfo )
        {
            Latitude = mapProjection.ScreenToLatitude( ScreenPoint ),
            Longitude = mapProjection.ScreenToLongitude( ScreenPoint )
        };
    }

    public LatLong LatLong { get; }
    public TilePoint TilePoint { get; }
    public DoublePoint ScreenPoint { get; }
}
