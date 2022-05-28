using System.Collections.Generic;
using System.Threading.Tasks;

namespace J4JSoftware.MapLibrary
{
    public interface IMapImageRetriever
    {
        bool IsInitialized { get; }
        IMapProjection MapProjection { get; set; }
        MapRetrieverInfo? MapRetrieverInfo { get; }
        bool FixedSizeImages { get; }

        Task<AsyncWebResult<List<MapImageData>>> GetMapImagesAsync(
            BoundingBox box,
            IEnumerable<MapTile>? existingTiles
        );

        Task<AsyncWebResult<MapImageData>> GetMapImageAsync(MapTile mapTile);
    }
}
