using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Web.Http;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;
using Microsoft.UI.Xaml.Media.Imaging;

namespace J4JSoftware.J4JMapControl;

public abstract class ImageDirectImageRetriever<TMultiTile> : MapImageRetriever<TMultiTile>
    where TMultiTile : ScreenTileGlobalCoordinates
{
    protected ImageDirectImageRetriever(
        ITileCollection tileCollection,
        IJ4JLogger? logger
    )
        : base( tileCollection, logger )
    {
    }

    protected override async Task<AsyncWebResult<BitmapImage, HttpStatusCode>> ExtractImageDataAsync( HttpResponseMessage response )
    {
        try
        {
            using var responseStream = await response.Content.ReadAsInputStreamAsync();
            var randomAccessStream = new InMemoryRandomAccessStream();

            await RandomAccessStream.CopyAsync(responseStream, randomAccessStream);
            randomAccessStream.Seek(0);

            var retVal = new BitmapImage();

            await retVal.SetSourceAsync(randomAccessStream);

            return new AsyncWebResult<BitmapImage, HttpStatusCode>( retVal, response.StatusCode );
        }
        catch (Exception ex)
        {
            Logger?.Error<string>("Could not set bitmap image, message was '{0}'", ex.Message);

            return new AsyncWebResult<BitmapImage, HttpStatusCode>( null,
                                                                    response.StatusCode,
                                                                    response.RequestMessage.RequestUri.AbsoluteUri,
                                                                    $"Could not set bitmap image, message was '{ex.Message}'" );
        }
    }
}
