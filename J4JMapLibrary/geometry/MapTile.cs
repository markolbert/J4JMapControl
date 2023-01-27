using System.Net;
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

            return x.Projection.Equals( y.Projection )
             && x.X == y.X
             && x.Y == y.Y;
        }

        public int GetHashCode( MapTile obj )
        {
            return HashCode.Combine( obj.Projection, obj.X, obj.Y );
        }
    }

    public static IEqualityComparer<MapTile> DefaultComparer { get; } = new TileProjectionXYEqualityComparer();

    #endregion

    public event EventHandler? ImageChanged;

    private readonly IJ4JLogger _logger;
    private MemoryStream? _imageStream;

    public MapTile(
        ITiledProjection projection,
        int x,
        int y,
        IJ4JLogger logger
    )
    {
        _logger = logger;
        _logger.SetLoggedType( GetType() );

        Projection = projection;
        Scale = projection.Scale;

        Center = new MapPoint( projection, _logger )
        {
            Cartesian =
            {
                X = x * Projection.TileHeightWidth + Projection.TileHeightWidth / 2,
                Y = y * Projection.TileHeightWidth + Projection.TileHeightWidth / 2
            }
        };

        X = x < 0 ? 0 : x;
        Y = y < 0 ? 0 : y;
        QuadKey = this.GetQuadKey();
    }

    public MapTile(
        ITiledProjection projection,
        Cartesian point,
        IJ4JLogger logger
    )
    {
        _logger = logger;
        _logger.SetLoggedType( GetType() );

        Projection = projection;
        Scale = projection.Scale;

        Center = new MapPoint( projection, _logger ) { Cartesian = { X = point.X, Y = point.Y } };

        X = point.X / projection.TileHeightWidth;
        Y = point.Y / projection.TileHeightWidth;
        QuadKey = this.GetQuadKey();
    }

    public MapTile(
        MapPoint center,
        IJ4JLogger logger
    )
    {
        _logger = logger;
        _logger.SetLoggedType( GetType() );

        Projection = center.Projection;
        Scale = center.Projection.Scale;

        Center = center;
        QuadKey = this.GetQuadKey();
    }

    public MapTile(
        LatLong latLong,
        IJ4JLogger logger
    )
    {
        _logger = logger;
        _logger.SetLoggedType( GetType() );

        Projection = latLong.Projection;
        Scale = latLong.Projection.Scale;

        Center = new MapPoint( Projection, _logger )
        {
            LatLong = { Latitude = latLong.Latitude, Longitude = latLong.Longitude }
        };

        QuadKey = this.GetQuadKey();
    }

    public ITiledProjection Projection { get; }
    public int Scale { get; }
    public bool ReflectsProjection => Scale == Projection.Scale;
    public bool CanBeCached => Projection.CanBeCached;

    public MapPoint Center { get; }
    public int HeightWidth => Projection.TileHeightWidth;
    public string QuadKey { get; }
    public int X { get; }
    public int Y { get; }

    public async Task<MemoryStream?> GetImageAsync( bool forceRetrieval = false )
    {
        if( !Projection.Initialized )
        {
            _logger.Error( "Projection is not initialized" );
            return null;
        }

        if( _imageStream != null && !forceRetrieval )
            return _imageStream;

        var wasNull = _imageStream == null;

        _imageStream = null;

        _logger.Verbose( "Beginning image retrieval from web" );

        if( !Projection.TryGetRequest( this, out var request ) )
        {
            _logger.Error( "Could not create HttpRequestMessage for tile ({0}, {1})", X, Y );
            if( wasNull )
                ImageChanged?.Invoke( this, EventArgs.Empty );

            return null;
        }

        var uriText = request!.RequestUri!.AbsoluteUri;
        var httpClient = new HttpClient();

        _logger.Verbose<string>( "Querying {0}", uriText );

        HttpResponseMessage? response;

        try
        {
            response = await httpClient.SendAsync( request );
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
                await response.Content.ReadAsStringAsync() );

            if( wasNull )
                ImageChanged?.Invoke( this, EventArgs.Empty );


            return null;
        }

        _logger.Verbose<string>( "Reading response from {0}", uriText );

        _imageStream = await Projection.ExtractImageDataAsync( response );
        ImageChanged?.Invoke( this, EventArgs.Empty );


        return _imageStream;
    }
}
