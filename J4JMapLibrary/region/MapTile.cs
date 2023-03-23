using System.Net;
using Serilog;

namespace J4JSoftware.J4JMapLibrary.MapRegion;

public class MapTile
{
    public static MapTile CreateMapTile(
        IProjection projection,
        int xTile,
        int yTile,
        int scale,
        ILogger logger
    )
    {
        var region = new MapRegion( projection, logger )
                    .Scale( scale )
                    .Size( projection.TileHeightWidth, projection.TileHeightWidth )
                    .Build();

        // determine the center point of the tile
        var upperLeftX = xTile * projection.TileHeightWidth;
        var upperLeftY = yTile * projection.TileHeightWidth;
        var centerPoint = new StaticPoint(projection) { Scale = scale };
        centerPoint.SetCartesian(upperLeftX + projection.TileHeightWidth / 2,
                                 upperLeftY + projection.TileHeightWidth / 2);

        region.CenterLatitude = centerPoint.Latitude;
        region.CenterLongitude = centerPoint.Longitude;
        region.RequestedHeight = (float)projection.TileHeightWidth / 2;
        region.RequestedWidth = (float)projection.TileHeightWidth / 2;

        return new MapTile(region, xTile, yTile);
    }

    private readonly ILogger _logger;

    public MapTile(
        MapRegion region,
        int x,
        int y
    )
    {
        _logger = region.Logger.ForContext<MapTile>();

        Region = region;
        X = x;
        Y = y;

        SetSize();
        NormalizeXY();

        QuadKey = this.GetQuadKey();

        FragmentId = region.Projection is ITiledProjection
            ? QuadKey
            : $"{MapExtensions.LatitudeToText( region.CenterLatitude )}-{MapExtensions.LongitudeToText( region.CenterLongitude )}-{region.Scale}-{region.Projection.TileHeightWidth}-{region.Projection.TileHeightWidth}";
    }

    private void NormalizeXY()
    {
        if (Region.Projection is ITiledProjection tiledProjection)
        {
            // x tile can wrap around forever...but we have to calculate
            // the wrapping differently for negative values because the C#
            // modulo operator behaves oddly
            var numTiles = tiledProjection.GetNumTiles( Region.Scale );

            RetrievedX = X;

            if (RetrievedX > 0)
                RetrievedX = X % numTiles;
            else
            {
                while (RetrievedX < 0)
                {
                    RetrievedX += numTiles;
                }
            }

            // y tile must lie between 0 and max tiles because 
            // the poles don't wrap around
            RetrievedY = Y < 0
                ? 0
                : Y > numTiles
                    ? numTiles - 1
                    : Y;
        }
        else
        {
            // static projections always return the same, single tile
            RetrievedX = 0;
            RetrievedY = 0;
        }
    }

    private void SetSize()
    {
        if( Region.Projection is ITiledProjection tiledProjection )
        {
            // for tiled projections, a MapTile's size equals
            // the underlying projection's tile size
            Height = tiledProjection.TileHeightWidth;
            Width = tiledProjection.TileHeightWidth;
        }
        else
        {
            // for static projections, there's only ever one
            // MapTile in the MapRegion, and its size
            // is that of the region's bounding box
            Height = Region.BoundingBox.Height;
            Width = Region.BoundingBox.Width;
        }
    }

    public MapRegion Region { get; }
    public int X { get; }
    public int Y { get; }

    public (int X, int Y) GetRelativeTileCoordinates()
    {
        return ( X - Region.UpperLeft.X, Y - Region.UpperLeft.Y );
    }

    // for tiled projections, to be in the projection Y must be between 0
    // and the maximum number of tiles along either projection axis.
    // X can be any value because the tiles wraparound

    // for static projections, X & Y must both be 0
    public bool InProjection => Region.Projection is ITiledProjection
        ? Region.Projection.GetTileRange( Region.Scale ).InRange( Y )
        : X == 0 && Y == 0;

    // RetrievedX and RetrievedY, in tiled projections, defines the actual
    // tile that was retrieved (which can be different from X and Y
    // because of horizontal wraparound and falling off the edge of the
    // map vertically)
    public int RetrievedX { get; private set; }
    public int RetrievedY { get; private set; }

    public float Height { get; set; }
    public float Width { get; set; }

    public string FragmentId { get; }
    public string QuadKey { get; }

    public byte[]? ImageData { get; set; }
    public long ImageBytes => ImageData?.Length <= 0 ? -1 : ImageData?.Length ?? -1;

    public async Task<bool> LoadImageAsync( CancellationToken ctx = default )
    {
        _logger.Verbose("Beginning image retrieval from web");

        var request = Region.Projection.CreateMessage(this);
        if (request == null)
        {
            _logger.Error("Could not create HttpRequestMessage for mapTile ({0})", FragmentId);
            return false;
        }

        var uriText = request.RequestUri!.AbsoluteUri;
        var httpClient = new HttpClient();

        _logger.Verbose("Querying {0}", uriText);

        HttpResponseMessage? response;

        try
        {
            response = Region.Projection.MaxRequestLatency <= 0
                ? await httpClient.SendAsync(request, ctx)
                : await httpClient.SendAsync(request, ctx)
                                  .WaitAsync(TimeSpan.FromMilliseconds(Region.Projection.MaxRequestLatency),
                                              ctx);

            _logger.Verbose("Got response from {0}", uriText);
        }
        catch (Exception ex)
        {
            _logger.Error("Image request from {0} failed, message was '{1}'",
                           request.RequestUri,
                           ex.Message);
            return false;
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            _logger.Error("Image request from {0} failed with response code {1}, message was '{2}'",
                           uriText,
                           response.StatusCode,
                           await response.Content.ReadAsStringAsync(ctx));

            return false;
        }

        _logger.Verbose("Reading response from {0}", uriText);

        // extract image data from response
        try
        {
            await using var responseStream = Region.Projection.MaxRequestLatency < 0
                ? await response.Content.ReadAsStreamAsync(ctx)
                : await response.Content.ReadAsStreamAsync(ctx)
                                .WaitAsync(TimeSpan.FromMilliseconds(Region.Projection.MaxRequestLatency),
                                            ctx);

            var memStream = new MemoryStream();
            await responseStream.CopyToAsync(memStream, ctx);

            ImageData = memStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.Error("Could not retrieve bitmap image stream from {0}, message was '{1}'",
                           response.RequestMessage!.RequestUri!,
                           ex.Message);

            return false;
        }

        return true;
    }

    public async Task<bool> LoadFromCacheAsync( ITileCache? cache, CancellationToken ctx = default )
    {
        var retVal = false;

        if( cache != null )
            retVal = await cache.LoadImageAsync( this, ctx );

        if( retVal )
            return true;

        return await LoadImageAsync( ctx );
    }
}
