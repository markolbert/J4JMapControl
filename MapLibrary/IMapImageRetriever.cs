namespace J4JSoftware.MapLibrary
{
    public interface IMapImageRetriever
    {
        MapRetrieverInfo? MapRetrieverInfo { get; }
        IZoom? Zoom { get; set; }

        Task<AsyncWebResult<List<object>, int>> GetMapImagesAsync(
            MapRect mapRectangle,
            IEnumerable<object>? existingCoords
        );

        Task<AsyncWebResult<object, int>> GetMapImageAsync(object tile);
    }
}
