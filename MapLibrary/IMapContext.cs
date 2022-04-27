namespace J4JSoftware.MapLibrary;

#pragma warning disable CS8618
public interface IMapContext
{
    IMapImageRetriever MapImageRetriever { get; }
    IZoom Zoom { get; }
    MapRect? ViewRect { get; }
}
