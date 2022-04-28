using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Web.Http;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;
using Microsoft.UI.Xaml.Media.Imaging;

namespace J4JSoftware.J4JMapControl;

public abstract class MapImageRetriever<TTile> : IMapImageRetriever
    where TTile : TileCoordinates
{
    protected MapImageRetriever(
        MapRetrieverInfo mapRetrieverInfo,
        IApplicationInfo appInfo,
        IJ4JLogger? logger
    )
    {
        MapRetrieverInfo = mapRetrieverInfo;
        AppInfo = appInfo;

        Logger = logger;
        Logger?.SetLoggedType( GetType() );
    }

    protected IJ4JLogger? Logger { get; }
    protected IApplicationInfo AppInfo { get; }

    public MapRetrieverInfo MapRetrieverInfo { get; }

    public async Task<ImageRetrievalResult> GetImageSourceAsync( TTile tile )
    {
        Logger?.Information("Beginning image retrieval from web");

        if( !TryGetRequest( tile, out var request ) )
            return new ImageRetrievalResult(null, "Could not create HttpRequestMessage for tile");

        var uriText = request!.RequestUri?.AbsoluteUri ?? "*** undefined Uri ***";
        var httpClient = new HttpClient();

        Logger?.Information<string>("Querying {0}", uriText);

        HttpResponseMessage? response;

        try
        {
            response = await httpClient.SendRequestAsync( request );
            Logger?.Information<string>("Got response from {0}", uriText);
        }
        catch (Exception ex)
        {
            Logger?.Error<string, string>("Image request from {0} failed, message was '{1}'",
                                           uriText,
                                           ex.Message);

            return new ImageRetrievalResult( null, $"Image request from {uriText} failed, message was '{ex.Message}'" );
        }

        if ( response.StatusCode != HttpStatusCode.Ok )
        {
            var error = await response.Content.ReadAsStringAsync();
            Logger?.Error<string, HttpStatusCode, string>( "Invalid response code from {0} ({1}), message was '{2}'",
                                                           uriText,
                                                           response.StatusCode,
                                                           error );

            return new ImageRetrievalResult( null,
                                             $"Image request from {uriText} failed with response code {response.StatusCode}, message was '{error}'" );
        }

        Logger?.Information<string>("Reading response from {0}", uriText);

        try
        {
            using var responseStream = await response.Content.ReadAsInputStreamAsync();
            var randomAccessStream = new InMemoryRandomAccessStream();

            await RandomAccessStream.CopyAsync(responseStream, randomAccessStream);
            randomAccessStream.Seek(0);

            var retVal = new BitmapImage();

            await retVal.SetSourceAsync(randomAccessStream);

            return new ImageRetrievalResult( retVal, null );
        }
        catch (Exception ex)
        {
            Logger?.Error<string>("Could not set bitmap image, message was '{0}'", ex.Message);

            return new ImageRetrievalResult(null, $"Could not set bitmap image, message was '{ex.Message}'");
        }
    }


    protected abstract Uri GetRequestUri(TTile tile);

    protected abstract bool TryGetRequest(TTile tile, out HttpRequestMessage? result);

    async Task<ImageRetrievalResult> IMapImageRetriever.GetImageSourceAsync( object tile )
    {
        if( tile is TTile castTile )
            return await GetImageSourceAsync( castTile );

        Logger?.Error( "GetMapImage requires a {0} but was provided a {1}", typeof( TTile ), tile.GetType() );

        return new ImageRetrievalResult( null,
                                         $"GetMapImage requires a {typeof( TTile )} but was provided a {tile.GetType()}" );
    }
}
