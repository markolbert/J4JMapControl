using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Web.Http;
using J4JSoftware.MapLibrary;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace J4JSoftware.J4JMapControl
{
    public interface IMapWinUiImageRetriever : IMapImageRetriever
    {
        Task<AsyncWebResult<MapImageData>> GetMapImageAsync( MultiCoordinates coordinates );
    }
}
