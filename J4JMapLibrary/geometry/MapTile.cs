using System.Net;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public record MapTile
{
    #region IEqualityComparer

    private sealed class TileProjectionXYEqualityComparer : IEqualityComparer<MapTile>
    {
        public bool Equals( MapTile? x, MapTile? y )
        {
            if( ReferenceEquals( x, y ) )
                return true;
            if( ReferenceEquals( x, null ) )
                return false;
            if( ReferenceEquals( y, null ) )
                return false;
            if( x.GetType() != y.GetType() )
                return false;

            return x.Metrics.Equals( y.Metrics )
             && x.X == y.X
             && x.Y == y.Y;
        }

        public int GetHashCode( MapTile obj )
        {
            return HashCode.Combine( obj.Metrics, obj.X, obj.Y );
        }
    }

    public static IEqualityComparer<MapTile> DefaultComparer { get; } = new TileProjectionXYEqualityComparer();

    #endregion

    public event EventHandler? ImageChanged;

    private readonly IJ4JLogger _logger;
    private readonly Func<MapTile, HttpRequestMessage?> _createRequest;
    private readonly Func<HttpResponseMessage, Task<MemoryStream?>> _extractImageStream;

    private MemoryStream? _imageStream;

    public MapTile(
        ITiledProjection projection,
        int x,
        int y
    )
    {
        _logger = J4JDeusEx.GetLogger<MapTile>()!;
        _createRequest = projection.GetRequest;
        _extractImageStream = projection.ExtractImageDataAsync;

        Metrics = projection.Metrics;
        Scale = projection.Scale;
        CanBeCached = projection.CanBeCached;
        MaxRequestLatency = projection.MaxRequestLatency;
        HeightWidth = projection.TileHeightWidth;

        Center = new MapPoint( Metrics )
        {
            Cartesian =
            {
                X = x * projection.TileHeightWidth + projection.TileHeightWidth / 2,
                Y = y * projection.TileHeightWidth + projection.TileHeightWidth / 2
            }
        };

        X = x < 0 ? 0 : x;
        Y = y < 0 ? 0 : y;
        QuadKey = this.ToQuadKey();
    }

    public MapTile(
        ITiledProjection projection,
        Cartesian point
    )
    {
        _logger = J4JDeusEx.GetLogger<MapTile>()!;
        _createRequest = projection.GetRequest;
        _extractImageStream = projection.ExtractImageDataAsync;

        Metrics = projection.Metrics;
        Scale = projection.Scale;
        CanBeCached = projection.CanBeCached;
        MaxRequestLatency = projection.MaxRequestLatency;
        HeightWidth = projection.TileHeightWidth;

        Center = new MapPoint( Metrics ) { Cartesian = { X = point.X, Y = point.Y } };

        X = point.X / projection.TileHeightWidth;
        Y = point.Y / projection.TileHeightWidth;
        QuadKey = this.ToQuadKey();
    }

    public MapTile(
        ITiledProjection projection,
        MapPoint center
    )
    {
        _logger = J4JDeusEx.GetLogger<MapTile>()!;
        _createRequest = projection.GetRequest;
        _extractImageStream = projection.ExtractImageDataAsync;

        Metrics = projection.Metrics;
        Scale = projection.Scale;
        CanBeCached = projection.CanBeCached;
        MaxRequestLatency = projection.MaxRequestLatency;
        HeightWidth = projection.TileHeightWidth;

        Center = center;
        QuadKey = this.ToQuadKey();
    }

    public MapTile(
        ITiledProjection projection,
        LatLong latLong
    )
    {
        _logger = J4JDeusEx.GetLogger<MapTile>()!;
        _createRequest = projection.GetRequest;
        _extractImageStream = projection.ExtractImageDataAsync;

        Metrics = projection.Metrics;
        Scale = projection.Scale;
        CanBeCached = projection.CanBeCached;
        MaxRequestLatency = projection.MaxRequestLatency;
        HeightWidth = projection.TileHeightWidth;

        Center = new MapPoint( Metrics )
        {
            LatLong = { Latitude = latLong.Latitude, Longitude = latLong.Longitude }
        };

        QuadKey = this.ToQuadKey();
    }

    public ProjectionMetrics Metrics { get; }
    public int Scale { get; }
    public bool ReflectsProjection => Scale == Metrics.Scale;
    public bool CanBeCached { get; }
    public int MaxRequestLatency { get; }

    public MapPoint Center { get; }
    public int HeightWidth { get; }
    public string QuadKey { get; }
    public int X { get; }
    public int Y { get; }

    public async Task<MemoryStream?> GetImageAsync( bool forceRetrieval = false ) =>
        await GetImageAsync( CancellationToken.None, forceRetrieval );

    public async Task<MemoryStream?> GetImageAsync(CancellationToken cancellationToken, bool forceRetrieval = false )
    {
        if( _imageStream != null && !forceRetrieval )
            return _imageStream;

        var wasNull = _imageStream == null;

        _imageStream = null;

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

        _imageStream = await _extractImageStream( response );
        ImageChanged?.Invoke( this, EventArgs.Empty );

        return _imageStream;
    }
}
