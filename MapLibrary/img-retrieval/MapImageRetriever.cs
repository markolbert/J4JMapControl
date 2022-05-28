using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Web.Http;
using J4JSoftware.Logging;

namespace J4JSoftware.MapLibrary;

public abstract class MapImageRetriever : IMapImageRetriever
{
    private IMapProjection? _mapProjection;

    protected MapImageRetriever(
        bool fixedSizeImages,
        IJ4JLogger? logger
    )
    {
        FixedSizeImages = fixedSizeImages;

        Logger = logger;
        Logger?.SetLoggedType( GetType() );
    }

    protected IJ4JLogger? Logger { get; }

    public bool IsInitialized => MapRetrieverInfo != null;

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
    public bool FixedSizeImages { get; }

    protected abstract MapRetrieverInfo? GetMapRetrieverInfo( IMapProjection mapProjection );

    public MapRetrieverInfo? MapRetrieverInfo { get; private set; }

    // Images returned by this call must each be decorated with:
    // * an attached DependencyProperty,TileCoordinatesProperty, containing information about the tile the Image is
    //   associated with
    // * an attached DependencyProperty, IsMapTileProperty, set equal to 'true'
    public async Task<AsyncWebResult<List<MapImageData>>> GetMapImagesAsync(
        BoundingBox box,
        IEnumerable<MapTile>? existingTiles = null
    )
    {
        if( !IsInitialized )
            return GetErrorAndLog<List<MapImageData>>( $"{this.GetType()} is not initialized, cannot retrieve images" );

        var images = new List<MapImageData>();

        existingTiles ??= Enumerable.Empty<MapTile>();
        var existing = existingTiles.ToList();

        foreach( var mapTile in box )
        {
            // don't retrieve Images we already have, if any were provided
            if( existing.Any( x => x == mapTile ) )
                continue;

            var result = await GetMapImageAsync( mapTile );

            if( result.ReturnValue == null )
                return GetErrorAndLog<List<MapImageData>>(
                    $"Failed to get image for {typeof( MapTile )}, message was '{result.Message}' (status code {result.HttpStatusCode}" );

            images.Add( result.ReturnValue );
        }

        if( images.Any())
            return new AsyncWebResult<List<MapImageData>>( images, (int) HttpStatusCode.Ok );

        return new AsyncWebResult<List<MapImageData>>( null,
                                                       (int) HttpStatusCode.BadRequest,
                                                       null,
                                                       "No tile images retrieved" );
    }

    // The Image returned by this call must be decorated with:
    // * an attached DependencyProperty, TileCoordinatesProperty, containing information about the tile the Image is
    //   associated with.
    // * an attached DependencyProperty, IsMapTileProperty, set equal to 'true'
    public async Task<AsyncWebResult<MapImageData>> GetMapImageAsync( MapTile mapTile )
    {
        if( !IsInitialized )
            return GetErrorAndLog<MapImageData>( $"{this.GetType()} is not initialized, cannot retrieve image" );

        Logger?.Information( "Beginning image retrieval from web" );

        var request = GetRequest( mapTile );
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
        if( imgData.ReturnValue == null )
            return GetErrorAndLog<MapImageData>( "Failed to retrieve image data" );

        return new AsyncWebResult<MapImageData>( new MapImageData(mapTile, imgData.ReturnValue!),
                                                                 (int) HttpStatusCode.Ok );
    }

    protected AsyncWebResult<TResult> GetErrorAndLog<TResult>(
        string msg,
        Uri? uri = null,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest
    )
    where TResult : class
    {
        Logger?.Error( msg );

        return new AsyncWebResult<TResult>( null,
                                            (int) statusCode,
                                            uri?.AbsoluteUri ?? null,
                                            msg );
    }

    protected abstract HttpRequestMessage? GetRequest( MapTile mapTile );

    protected abstract Task<AsyncWebResult<InMemoryRandomAccessStream>> ExtractImageDataAsync(
        HttpResponseMessage response );
}
