namespace J4JSoftware.MapLibrary;

public record ScreenTileGlobalCoordinates(
    DoublePoint ScreenUpperLeft,
    IntPoint TileCoordinates,
    LatLong GlobeCoordinates,
    IZoom Zoom
) : Coordinates( ScreenUpperLeft, Zoom );