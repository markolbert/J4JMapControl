using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Web.Http;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;
using Serilog;

namespace J4JSoftware.J4JMapControl;

public abstract class MapImageRetriever<TCoord> : IMapImageRetriever<TCoord>
    where TCoord : Coordinates
{
    private IMapProjection? _mapProjection;
    private MapRetrieverInfo? _mapRetrieverInfo;

    protected MapImageRetriever(
        IJ4JLogger? logger
    )
    {
        Logger = logger;
        Logger?.SetLoggedType( GetType() );
    }

    protected IJ4JLogger? Logger { get; }

    public IMapProjection MapProjection
    {
        get
        {
            if( _mapProjection != null )
                return _mapProjection;

            var msg = $"Trying to access {nameof( MapProjection )} when it has not been initialized";

            Logger?.Fatal( msg );

            throw new NullReferenceException( msg );
        }

        set
        {
            _mapProjection = value;

            MapRetrieverInfo = GetMapRetrieverInfo( _mapProjection );
        }
    }

    protected abstract MapRetrieverInfo GetMapRetrieverInfo( IMapProjection mapProjection );

    public MapRetrieverInfo MapRetrieverInfo
    {
        get
        {
            if( _mapRetrieverInfo != null )
                return _mapRetrieverInfo;

            var msg = $"Trying to access {nameof( MapRetrieverInfo )} when it has not been initialized";

            Logger?.Fatal( msg );

            throw new NullReferenceException( msg );
        }

        private set => _mapRetrieverInfo = value;
    }

    // Images returned by this call must each be decorated with:
    // * an attached DependencyProperty,TileCoordinatesProperty, containing information about the tile the Image is
    //   associated with
    // * an attached DependencyProperty, IsMapTileProperty, set equal to 'true'
    public async Task<AsyncWebResult<List<MapImageData>, HttpStatusCode>> GetMapImagesAsync(
        BoundingBox box,
        IEnumerable<TCoord>? existingCoords = null
    )
    {
        var images = new List<MapImageData>();

        existingCoords ??= Enumerable.Empty<TCoord>();
        var existing = existingCoords.ToList();

        foreach( var coordinate in GetCoordinateIterator( box ) )
        {
            // don't retrieve Images we already have, if any were provided
            if( existing.Any( x => x == coordinate ) )
                continue;

            var result = await GetMapImageAsync( coordinate );

            if( result.ReturnValue == null )
                return GetErrorAndLog<List<MapImageData>>(
                    $"Failed to get image for {typeof( TCoord )}, message was '{result.Message}' (status code {result.HttpStatusCode}" );

            images.Add( result.ReturnValue );
        }

        return new AsyncWebResult<List<MapImageData>, HttpStatusCode>( images, HttpStatusCode.Ok );
    }

    protected abstract IEnumerable<TCoord> GetCoordinateIterator( BoundingBox box );

    // The Image returned by this call must be decorated with:
    // * an attached DependencyProperty, TileCoordinatesProperty, containing information about the tile the Image is
    //   associated with.
    // * an attached DependencyProperty, IsMapTileProperty, set equal to 'true'
    public async Task<AsyncWebResult<MapImageData, HttpStatusCode>> GetMapImageAsync( TCoord coordinates )
    {
        Logger?.Information( "Beginning image retrieval from web" );

        var request = GetRequest( coordinates );
        if( request == null )
            return GetErrorAndLog<MapImageData>( "Could not create HttpRequestMessage for tile" );

        var uriText = request.RequestUri?.AbsoluteUri ?? "*** undefined Uri ***";
        var httpClient = new HttpClient();

        Logger?.Information<string>( "Querying {0}", uriText );

        HttpResponseMessage? response = null;

        try
        {
            response = await httpClient.SendRequestAsync( request );
            Logger?.Information<string>( "Got response from {0}", uriText );
        }
        catch( Exception ex )
        {
            return GetErrorAndLog<MapImageData>( $"Image request from {uriText} failed, message was '{ex.Message}'",
                                                 request.RequestUri,
                                                 response?.StatusCode ?? HttpStatusCode.BadRequest );
        }

        if( response.StatusCode != HttpStatusCode.Ok )
        {
            var error = await response.Content.ReadAsStringAsync();

            return GetErrorAndLog<MapImageData>(
                $"Image request from {uriText} failed with response code {response.StatusCode}, message was '{error}'",
                request.RequestUri,
                response.StatusCode );
        }

        Logger?.Information<string>( "Reading response from {0}", uriText );

        var imgData = await ExtractImageDataAsync( response );
        if( !imgData.IsValid )
            return GetErrorAndLog<MapImageData>( "Failed to retrieve image data" );

        return new AsyncWebResult<MapImageData, HttpStatusCode>( new MapImageData(coordinates, imgData.ReturnValue!),
                                                                 HttpStatusCode.Ok );
    }

    protected AsyncWebResult<TResult, HttpStatusCode> GetErrorAndLog<TResult>(
        string msg,
        Uri? uri = null,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest
    )
    where TResult : class
    {
        Logger?.Error( msg );

        return new AsyncWebResult<TResult, HttpStatusCode>(null,
                                                         statusCode,
                                                         uri?.AbsoluteUri ?? null,
                                                         msg);
    }

    protected abstract HttpRequestMessage? GetRequest( TCoord coordinates );

    protected abstract Task<AsyncWebResult<InMemoryRandomAccessStream, HttpStatusCode>> ExtractImageDataAsync(
        HttpResponseMessage response );

    async Task<AsyncWebResult<List<object>, int>> IMapImageRetriever.GetMapImagesAsync(
        BoundingBox box,
        IEnumerable<object>? existingImages
    )
    {
        var existingOkay = true;

        var existingCast = new List<TCoord>();

        foreach( var existing in existingImages ?? Enumerable.Empty<object>() )
        {
            if( existing is TCoord coord )
                existingCast.Add( coord );
            else existingOkay = false;
        }

        var retVal = await GetMapImagesAsync( box, existingOkay ? existingCast : null );

        if( !retVal.IsValid )
            return new AsyncWebResult<List<object>, int>( null, (int) HttpStatusCode.BadRequest );

        return new AsyncWebResult<List<object>, int>( retVal.ReturnValue!.Cast<object>().ToList(),
                                                      (int) retVal.HttpStatusCode,
                                                      retVal.Url,
                                                      retVal.Message );
    }

    async Task<AsyncWebResult<object, int>> IMapImageRetriever.GetMapImageAsync( object tile )
    {
        if( tile is TCoord castTile )
        {
            var retVal = await GetMapImageAsync( castTile );

            return new AsyncWebResult<object, int>( retVal.ReturnValue, (int) retVal.HttpStatusCode, retVal.Url, retVal.Message );
        }

        Logger?.Error( "GetMapImage requires a {0} but was provided a {1}", typeof( TCoord ), tile.GetType() );

        return new AsyncWebResult<object, int>( null,
                                                (int) HttpStatusCode.BadRequest,
                                                Message:
                                                $"GetMapImage requires a {typeof( TCoord )} but was provided a {tile.GetType()}" );
    }
}
