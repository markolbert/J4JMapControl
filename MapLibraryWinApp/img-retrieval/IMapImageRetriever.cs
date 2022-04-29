using System.Threading.Tasks;
using Windows.Web.Http;
using J4JSoftware.MapLibrary;
using Microsoft.UI.Xaml.Media.Imaging;

namespace J4JSoftware.J4JMapControl
{
    public interface IMapImageRetriever<in TTile> : IMapImageRetriever
        where TTile : TileCoordinates
    {
        ITileCollection? TileCollection { get; }

        Task<AsyncWebResult<BitmapImage, HttpStatusCode>> GetImageSourceAsync( TTile tile );
    }
}
