namespace J4JSoftware.MapLibrary
{
    public interface IMapImageRetriever
    {
        IMapProjection MapProjection { get; set; }
        MapRetrieverInfo MapRetrieverInfo { get; }

        Task<AsyncWebResult<List<object>, int>> GetMapImagesAsync(
            BoundingBox box,
            IEnumerable<object>? existingImages
        );

        Task<AsyncWebResult<object, int>> GetMapImageAsync(object tile);
    }
}
