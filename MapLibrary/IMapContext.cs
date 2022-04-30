namespace J4JSoftware.MapLibrary;

public interface IMapContext
{
    IMapImageRetriever MapRetriever { get; }
    double Height { get; }
    double Width { get; }
}
