using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Web.Http;
using J4JSoftware.MapLibrary;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace J4JSoftware.J4JMapControl
{
    public interface IMapImageRetriever<in TCoord> : IMapImageRetriever
        where TCoord : Coordinates
    {
        Task<AsyncWebResult<List<Image>, HttpStatusCode>> GetMapImagesAsync( MapRect mapRectangle, IEnumerable<TCoord>? existingCoords = null );
        Task<AsyncWebResult<Image, HttpStatusCode>> GetMapImageAsync( TCoord coordinates );
    }
}
