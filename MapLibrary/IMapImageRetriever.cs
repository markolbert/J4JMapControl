namespace J4JSoftware.MapLibrary
{
    public interface IMapImageRetriever
    {
        MapRetrieverInfo? MapRetrieverInfo { get; }
        IZoom? Zoom { get; }

        Task<AsyncWebResult<object, int>> GetImageSourceAsync(object tile);
    }
}
