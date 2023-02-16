using System.Net;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract class MapFragment : IMapFragment
{
    public event EventHandler? ImageChanged;

    protected MapFragment(
        IProjection projection
    )
    {
        Logger = J4JDeusEx.GetLogger();
        Logger?.SetLoggedType( GetType() );

        MapServer = projection.MapServer;

        MaxRequestLatency = projection.MapServer.MaxRequestLatency;
    }

    protected IJ4JLogger? Logger { get; }

    public IMapServer MapServer { get; }
    public int MaxRequestLatency { get; }

    public abstract string FragmentId { get; }

    public byte[]? ImageData { get; protected set; }
    public long ImageBytes { get; private set; } = -1L;

    public async Task<byte[]?> GetImageAsync( int scale, bool forceRetrieval = false, CancellationToken ctx = default )
    {
        if( ImageData != null && !forceRetrieval )
            return ImageData;

        var wasNull = ImageData == null;

        ImageData = null;
        ImageBytes = -1L;

        Logger?.Verbose( "Beginning image retrieval from web" );

        var request = MapServer.CreateMessage( this, scale );
        if( request == null )
        {
            Logger?.Error<string>( "Could not create HttpRequestMessage for mapFragment ({0})", FragmentId );
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
}
