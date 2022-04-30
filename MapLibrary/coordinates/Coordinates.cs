namespace J4JSoftware.MapLibrary;

public record Coordinates
{
    protected Coordinates(
        DoublePoint upperLeft,
        IZoom zoom
    )
    {
        ScreenUpperLeft = upperLeft;
        Zoom = zoom;
    }

    public DoublePoint ScreenUpperLeft { get; }
    public IZoom Zoom { get; }
}
