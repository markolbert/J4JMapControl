namespace J4JSoftware.MapLibrary
{
    public interface IMapImageRetriever
    {
        MapRetrieverInfo MapRetrieverInfo { get; }

        Task<ImageRetrievalResult> GetImageSourceAsync(object tile);
    }
}
