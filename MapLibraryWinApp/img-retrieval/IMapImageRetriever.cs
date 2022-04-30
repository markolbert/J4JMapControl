using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Web.Http;
using J4JSoftware.MapLibrary;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace J4JSoftware.J4JMapControl
{
    public interface IMapImageRetriever<in TTile> : IMapImageRetriever
        where TTile : Coordinates
    {
        ITileCollection? TileCollection { get; }

        Task<AsyncWebResult<Image, HttpStatusCode>> GetImageSourceAsync( TTile coordinates );
    }
}
