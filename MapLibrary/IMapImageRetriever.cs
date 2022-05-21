namespace J4JSoftware.MapLibrary
{
    public interface IMapImageRetriever
    {
        bool IsInitialized { get; }
        IMapProjection MapProjection { get; set; }
        MapRetrieverInfo? MapRetrieverInfo { get; }
        bool FixedSizeImages { get; }

        Task<AsyncWebResult<List<object>>> GetMapImagesAsync(
            BoundingBox box,
            IEnumerable<MultiCoordinates>? existingImages
        );

        Task<AsyncWebResult<object>> GetMapImageAsync(object tile);
    }
}
