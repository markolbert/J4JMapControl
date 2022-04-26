namespace J4JSoftware.MapLibrary;

public record MultiTileCoordinates(
    IntPoint ScreenUpperLeft,
    IntPoint TileCoordinates,
    LatLong GlobeCoordinates,
    IZoom Zoom 
    ) : TileCoordinates( ScreenUpperLeft, Zoom );