namespace J4JSoftware.MapLibrary;

public interface IMapContext
{
    event EventHandler? UpdateMap;

    IMapImageRetriever MapRetriever { get; }
    IZoom Zoom { get; }
    double Height { get; }
    double Width { get; }
}
