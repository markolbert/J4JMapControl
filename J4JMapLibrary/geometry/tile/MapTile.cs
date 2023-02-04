using System.Net;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public partial class MapTile
{
    public event EventHandler? ImageChanged;

    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly IJ4JLogger _logger;
    private readonly Func<MapTile, HttpRequestMessage?> _createRequest;
    private readonly Func<HttpResponseMessage, Task<byte[]?>> _extractImageStreamAsync;

    private byte[]? _imageData;

    public ProjectionMetrics Metrics { get; }
    public int Scale { get; }
    public int MaxRequestLatency { get; }

    public int HeightWidth { get; }
    public string QuadKey { get; }
    public int X { get; }
    public int Y { get; }

    public long ImageBytes { get; private set; } = -1L;

    public async Task<byte[]?> GetImageAsync( bool forceRetrieval = false )
    {
        return await GetImageAsync( _cancellationTokenSource.Token, forceRetrieval );
    }

    public async Task<byte[]?> GetImageAsync(CancellationToken cancellationToken, bool forceRetrieval = false )
    {
        if( _imageData != null && !forceRetrieval )
            return _imageData;

        var wasNull = _imageData == null;

        _imageData = null;
        ImageBytes = -1L;

        _logger.Verbose( "Beginning image retrieval from web" );

        var request = _createRequest( this );
        if( request == null )
        {
            _logger.Error( "Could not create HttpRequestMessage for tile ({0}, {1})", X, Y );
            if( wasNull )
                ImageChanged?.Invoke( this, EventArgs.Empty );

            return null;
        }

        var uriText = request.RequestUri!.AbsoluteUri;
        var httpClient = new HttpClient();

        _logger.Verbose<string>( "Querying {0}", uriText );

        HttpResponseMessage? response;

        try
        {
            response = MaxRequestLatency <= 0
                ? await httpClient.SendAsync( request, cancellationToken )
                : await httpClient.SendAsync( request, cancellationToken )
                                  .WaitAsync( TimeSpan.FromMilliseconds( MaxRequestLatency ), cancellationToken );

            _logger.Verbose<string>( "Got response from {0}", uriText );
        }
        catch( Exception ex )
        {
            _logger.Error<Uri, string>( "Image request from {0} failed, message was '{1}'",
                                        request.RequestUri,
                                        ex.Message );
            if( wasNull )
                ImageChanged?.Invoke( this, EventArgs.Empty );

            return null;
        }

        if( response.StatusCode != HttpStatusCode.OK )
        {
            _logger.Error<string, HttpStatusCode, string>(
                "Image request from {0} failed with response code {1}, message was '{2}'",
                uriText,
                response.StatusCode,
                await response.Content.ReadAsStringAsync( cancellationToken ) );

            if( wasNull )
                ImageChanged?.Invoke( this, EventArgs.Empty );


            return null;
        }

        _logger.Verbose<string>( "Reading response from {0}", uriText );

        _imageData = await _extractImageStreamAsync( response );
        ImageChanged?.Invoke(this, EventArgs.Empty);

        if ( _imageData == null )
            return null;

        ImageBytes = _imageData.Length;

        return _imageData;
    }
}
