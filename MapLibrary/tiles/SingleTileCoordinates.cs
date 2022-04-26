namespace J4JSoftware.MapLibrary;

public record SingleTileCoordinates(
    IntPoint ScreenUpperLeft, 
    IntPoint ScreenLowerRight,
    LatLong UpperLeft, 
    LatLong LowerRight, 
    IZoom Zoom 
) : TileCoordinates(ScreenUpperLeft, Zoom);
