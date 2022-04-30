using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Web.Http;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;
using Microsoft.UI.Xaml.Controls;
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

    protected override async Task<AsyncWebResult<Image, HttpStatusCode>> ExtractImageDataAsync( HttpResponseMessage response, TMultiTile coordinates )
    {
        try
        {
            using var responseStream = await response.Content.ReadAsInputStreamAsync();
            var randomAccessStream = new InMemoryRandomAccessStream();

            await RandomAccessStream.CopyAsync(responseStream, randomAccessStream);
            randomAccessStream.Seek(0);

            //return new AsyncWebResult<InMemoryRandomAccessStream, HttpStatusCode>(
            //    randomAccessStream,
            //    response.StatusCode );

            var retVal = new Image();

            var source = new BitmapImage();
            await source.SetSourceAsync(randomAccessStream);

            retVal.Source = source;
            AttachedProperties.SetTileCoordinates( retVal, coordinates );

            return new AsyncWebResult<Image, HttpStatusCode>(retVal, response.StatusCode);
        }
        catch (Exception ex)
        {
            Logger?.Error<string>("Could not set bitmap image, message was '{0}'", ex.Message);

            return new AsyncWebResult<Image, HttpStatusCode>( null,
                                                                    response.StatusCode,
                                                                    response.RequestMessage.RequestUri.AbsoluteUri,
                                                                    $"Could not set bitmap image, message was '{ex.Message}'" );
        }
    }
}
