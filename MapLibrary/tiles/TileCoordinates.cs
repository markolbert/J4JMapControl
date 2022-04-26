namespace J4JSoftware.MapLibrary;

public record TileCoordinates
{
    protected TileCoordinates(
        IntPoint upperLeft,
        IZoom zoom
    )
    {
        ScreenUpperLeft = upperLeft;
        Zoom = zoom;
    }

    public IntPoint ScreenUpperLeft { get; }
    public IZoom Zoom { get; }
}
