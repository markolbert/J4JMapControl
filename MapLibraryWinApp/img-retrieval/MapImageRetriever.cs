using System;
using System.Threading.Tasks;
using Windows.Web.Http;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;
using Microsoft.UI.Xaml.Media.Imaging;

namespace J4JSoftware.J4JMapControl;

public abstract class MapImageRetriever<TTile> : IMapImageRetriever<TTile>
    where TTile : Coordinates
{
    private MapRetrieverInfo? _retrieverInfo;
    private IZoom? _zoom;

    protected MapImageRetriever(
        ITileCollection tileCollection,
        IJ4JLogger? logger
    )
    {
        TileCollection = tileCollection;

        Logger = logger;
        Logger?.SetLoggedType( GetType() );
    }

    protected IJ4JLogger? Logger { get; }

    public virtual MapRetrieverInfo? MapRetrieverInfo => _retrieverInfo;

    protected void SetRetrieverInfo( MapRetrieverInfo retrieverInfo )
    {
        _retrieverInfo = retrieverInfo;

        Zoom = new Zoom( retrieverInfo );
    }

    public IZoom? Zoom
    {
        get
        {
            if( _zoom != null )
                return _zoom;

            var msg = $"Trying to access an unconfigured {typeof(IZoom)}";

            Logger?.Fatal(msg);
            J4JDeusEx.OutputFatalMessage(msg, null);

            throw new J4JDeusExException(msg);
        }

        private set => _zoom = value;
    }

    public ITileCollection TileCollection { get; }

    public async Task<AsyncWebResult<BitmapImage, HttpStatusCode>> GetImageSourceAsync( TTile tile )
    {
        Logger?.Information("Beginning image retrieval from web");

        var request = GetRequest( tile );
        if( request == null )
            return new AsyncWebResult<BitmapImage, HttpStatusCode>( null,
                                                                    HttpStatusCode.BadRequest,
                                                                    Message:
                                                                    "Could not create HttpRequestMessage for tile" );

        var uriText = request.RequestUri?.AbsoluteUri ?? "*** undefined Uri ***";
        var httpClient = new HttpClient();

        Logger?.Information<string>("Querying {0}", uriText);

        HttpResponseMessage? response = null;

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

            return new AsyncWebResult<BitmapImage, HttpStatusCode>( null,
                                                                    response?.StatusCode ?? HttpStatusCode.BadRequest,
                                                                    uriText,
                                                                    $"Image request from {uriText} failed, message was '{ex.Message}'" );
        }

        if ( response.StatusCode != HttpStatusCode.Ok )
        {
            var error = await response.Content.ReadAsStringAsync();
            Logger?.Error<string, HttpStatusCode, string>( "Invalid response code from {0} ({1}), message was '{2}'",
                                                           uriText,
                                                           response.StatusCode,
                                                           error );

            return new AsyncWebResult<BitmapImage, HttpStatusCode>( null,
                                                                    response.StatusCode,
                                                                    uriText,
                                                                    $"Image request from {uriText} failed with response code {response.StatusCode}, message was '{error}'" );
        }

        Logger?.Information<string>("Reading response from {0}", uriText);

        return await ExtractImageDataAsync( response );
    }

    protected abstract Task<AsyncWebResult<BitmapImage, HttpStatusCode>> ExtractImageDataAsync(
        HttpResponseMessage response
    );
    
    protected abstract HttpRequestMessage? GetRequest(TTile tile);

    async Task<AsyncWebResult<object, int>> IMapImageRetriever.GetImageSourceAsync( object tile )
    {
        if( tile is TTile castTile )
        {
            var retVal = await GetImageSourceAsync( castTile );

            return new AsyncWebResult<object, int>( retVal.ReturnValue, (int) retVal.HttpStatusCode, retVal.Url, retVal.Message );
        }

        Logger?.Error( "GetMapImage requires a {0} but was provided a {1}", typeof( TTile ), tile.GetType() );

        return new AsyncWebResult<object, int>( null,
                                                (int) HttpStatusCode.BadRequest,
                                                Message:
                                                $"GetMapImage requires a {typeof( TTile )} but was provided a {tile.GetType()}" );
    }

    object IMapImageRetriever.GetTileCollection() => TileCollection;
}
