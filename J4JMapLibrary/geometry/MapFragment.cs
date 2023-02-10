using System.Net;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract class MapFragment<TScope> : IMapTile<TScope>
    where TScope : MapScope
{
    private readonly CancellationTokenSource _ctxSource = new();

    protected MapFragment(
        IMapProjection projection
    )
    {
        Logger = J4JDeusEx.GetLogger();
        Logger?.SetLoggedType( GetType() );

        var temp = projection.GetScope() as TScope;
        if( temp == null )
        {
            var mesg = $"Could not retrieve {0} from projection";
            Logger?.Fatal( mesg );
            throw new NullReferenceException( mesg );
        }

        var temp2 = temp switch
        {
            TiledScope tiledScope => TiledScope.Copy( tiledScope ) as TScope,
            MapScope mapScope => MapScope.Copy( mapScope ) as TScope,
            _ => throw new InvalidOperationException( $"Unsupported MapScope type '{typeof( TScope )}'" )
        };

        Scope = temp2!;
        MapServer = projection.MapServer;

        MaxRequestLatency = projection.MapServer.MaxRequestLatency;
    }

    protected IJ4JLogger? Logger { get; }
    protected abstract string TileId { get; }
    protected byte[]? ImageData { get; set; }

    public TScope Scope { get; }
    public IMapServer MapServer { get; }
    public int MaxRequestLatency { get; }

    public long ImageBytes { get; private set; } = -1L;
    public event EventHandler? ImageChanged;

    public async Task<byte[]?> GetImageAsync( bool forceRetrieval = false, CancellationToken ctx = default )
    {
        if( ImageData != null && !forceRetrieval )
            return ImageData;

        var wasNull = ImageData == null;

        ImageData = null;
        ImageBytes = -1L;

        Logger?.Verbose( "Beginning image retrieval from web" );

        var request = MapServer.CreateMessage( this );
        if( request == null )
        {
            Logger?.Error<string>( "Could not create HttpRequestMessage for mapFragment ({0})", TileId );
            if( wasNull )
                ImageChanged?.Invoke( this, EventArgs.Empty );

            return null;
        }

        var uriText = request.RequestUri!.AbsoluteUri;
        var httpClient = new HttpClient();

        Logger?.Verbose<string>( "Querying {0}", uriText );

        HttpResponseMessage? response;

        try
        {
            response = MaxRequestLatency <= 0
                ? await httpClient.SendAsync( request, ctx )
                : await httpClient.SendAsync( request, ctx )
                                  .WaitAsync( TimeSpan.FromMilliseconds( MaxRequestLatency ), ctx );

            Logger?.Verbose<string>( "Got response from {0}", uriText );
        }
        catch( Exception ex )
        {
            Logger?.Error<Uri, string>( "Image request from {0} failed, message was '{1}'",
                                        request.RequestUri,
                                        ex.Message );
            if( wasNull )
                ImageChanged?.Invoke( this, EventArgs.Empty );

            return null;
        }

        if( response.StatusCode != HttpStatusCode.OK )
        {
            Logger?.Error<string, HttpStatusCode, string>(
                "Image request from {0} failed with response code {1}, message was '{2}'",
                uriText,
                response.StatusCode,
                await response.Content.ReadAsStringAsync( ctx ) );

            if( wasNull )
                ImageChanged?.Invoke( this, EventArgs.Empty );

            return null;
        }

        Logger?.Verbose<string>( "Reading response from {0}", uriText );

        ImageData = await ExtractImageStreamAsync( response, ctx );
        ImageChanged?.Invoke( this, EventArgs.Empty );

        if( ImageData == null )
            return null;

        ImageBytes = ImageData.Length;

        return ImageData;
    }

    protected virtual async Task<byte[]?> ExtractImageStreamAsync(
        HttpResponseMessage response,
        CancellationToken ctx = default
    )
    {
        try
        {
            await using var responseStream = MaxRequestLatency < 0
                ? await response.Content.ReadAsStreamAsync( ctx )
                : await response.Content.ReadAsStreamAsync( ctx )
                                .WaitAsync( TimeSpan.FromMilliseconds( MaxRequestLatency ), ctx );

            var memStream = new MemoryStream();
            await responseStream.CopyToAsync( memStream, ctx );

            return memStream.ToArray();
        }
        catch( Exception ex )
        {
            Logger?.Error<Uri, string>( "Could not retrieve bitmap image stream from {0}, message was '{1}'",
                                        response.RequestMessage!.RequestUri!,
                                        ex.Message );

            return null;
        }
    }

    MapScope IMapFragment.GetScope() => Scope;
}
