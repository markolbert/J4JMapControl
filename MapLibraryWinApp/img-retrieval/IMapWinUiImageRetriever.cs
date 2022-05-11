using System.Threading.Tasks;
using J4JSoftware.MapLibrary;

namespace J4JSoftware.J4JMapControl
{
    public interface IMapWinUiImageRetriever : IMapImageRetriever
    {
        Task<AsyncWebResult<MapImageData>> GetMapImageAsync( MultiCoordinates coordinates );
    }
}
