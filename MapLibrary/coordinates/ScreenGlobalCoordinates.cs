namespace J4JSoftware.MapLibrary;

public record ScreenGlobalCoordinates(
    DoublePoint ScreenUpperLeft, 
    DoublePoint ScreenLowerRight,
    LatLong UpperLeft, 
    LatLong LowerRight, 
    IZoom Zoom 
) : Coordinates(ScreenUpperLeft, Zoom);
