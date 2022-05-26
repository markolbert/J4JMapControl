using System.Collections.Generic;
using System.Threading.Tasks;
using J4JSoftware.MapLibrary;

namespace J4JSoftware.J4JMapControl
{
    public interface IMapImageRetriever
    {
        bool IsInitialized { get; }
        IMapProjection MapProjection { get; set; }
        MapRetrieverInfo? MapRetrieverInfo { get; }
        bool FixedSizeImages { get; }

        Task<AsyncWebResult<List<MapImageData>>> GetMapImagesAsync(
            BoundingBox box,
            IEnumerable<MultiCoordinates>? existingImages
        );

        Task<AsyncWebResult<MapImageData>> GetMapImageAsync(MultiCoordinates coordinates);
    }
}
