namespace J4JSoftware.MapLibrary
{
    public interface IMapImageRetriever
    {
        MapRetrieverInfo? MapRetrieverInfo { get; }
        IZoom? Zoom { get; set; }

        Task<AsyncWebResult<object, int>> GetImageStreamAsync(object tile);
        ITileCollection GetTileCollection();
    }
}
