using System.Threading.Tasks;
using Windows.Web.Http;
using J4JSoftware.MapLibrary;
using Microsoft.UI.Xaml.Media.Imaging;

namespace J4JSoftware.J4JMapControl
{
    public interface IRetrieveMapImage<in TTile> : IMapImageRetriever
        where TTile : TileCoordinates
    {
        Task<AsyncWebResult<BitmapImage, HttpStatusCode>> GetImageSourceAsync( TTile tile );
    }
}
