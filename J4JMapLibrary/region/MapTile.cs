﻿using System.Net;
using System.Text;
using Serilog;

namespace J4JSoftware.J4JMapLibrary.MapRegion;

public class MapTile : Tile
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

    public MapTile(
        MapRegion region,
        int x,
        int y
    )
        : base( region, x, y )
    {
        SetSize();
        
        QuadKey = InProjection ? GetQuadKey() : string.Empty;

        FragmentId = region.Projection is ITiledProjection
            ? QuadKey
            : $"{MapExtensions.LatitudeToText( region.CenterLatitude )}-{MapExtensions.LongitudeToText( region.CenterLongitude )}-{region.Scale}-{region.Projection.TileHeightWidth}-{region.Projection.TileHeightWidth}";
    }

    private string GetQuadKey()
    {
        // static projections only have a single quadkey, defaulting to "0"
        if (Region.Projection is not ITiledProjection)
            return "0";

        var retVal = new StringBuilder();

        for (var i = Region.Scale; i > Region.Projection.ScaleRange.Minimum - 1; i--)
        {
            var digit = '0';
            var mask = 1 << (i - 1);

            if ((X & mask) != 0)
                digit++;

            if ((Y & mask) != 0)
            {
                digit++;
                digit++;
            }

            retVal.Append(digit);
        }

        return retVal.ToString();
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

    public bool InProjection
    {
        get
        {
            switch( Region.ProjectionType )
            {
                case ProjectionType.Static:
                    return X == 0 && Y == 0;

                case ProjectionType.Tiled:
                    var tileRange = Region.Projection.GetTileRange( Region.Scale );
                    return tileRange.InRange( X ) && tileRange.InRange( Y );

                default:
                    return false;
            }
        }
    }

    public float Height { get; set; }
    public float Width { get; set; }

    public string FragmentId { get; }
    public string QuadKey { get; }

    public (int X, int Y) GetRelativeTileCoordinates()
    {
        if (!Region.IsDefined)
            return (-1, -1);

        return (X - Region.UpperLeft.X, Y - Region.UpperLeft.Y);
    }

    //public (int X, int Y) AbsoluteTileCoordinates
    //{
    //    get
    //    {
    //        if (Region.ProjectionType == ProjectionType.Static)
    //            return (X, Y);

    //        var numTiles = Region.Projection.GetNumTiles(Region.Scale);

    //        var leftMostIncluded = Region.UpperLeft.X <= 0
    //            ? 0
    //            : Region.UpperLeft.X > numTiles - 1
    //                ? -1
    //                : Region.UpperLeft.X;

    //        var rightMostIncluded = Region.LowerRight.X >= numTiles - 1
    //            ? numTiles - 1
    //            : Region.LowerRight.X < 0
    //                ? -1
    //                : Region.LowerRight.X;

    //        var normalizedX = NormalizedX(X, numTiles);

    //        if (normalizedX < 0 || normalizedX > numTiles - 1)
    //            return (-1, Y);

    //        if (normalizedX > rightMostIncluded || normalizedX <= leftMostIncluded)
    //            return (normalizedX, Y);

    //        return (-1, Y);
    //    }
    //}

    //private int NormalizedX(int value, int numTiles) =>
    //    value < 0
    //        ? value + numTiles
    //        : value > numTiles - 1
    //            ? value - numTiles
    //            : value;

    public byte[]? ImageData { get; set; }
    public long ImageBytes => ImageData?.Length <= 0 ? -1 : ImageData?.Length ?? -1;

    public byte[]? GetImage()
    {
        return Task.Run(async () => await GetImageAsync()).Result;
    }

    public async Task<byte[]?> GetImageAsync(CancellationToken ctx = default)
    {
        if( !InProjection )
            return null;

        Logger.Verbose("Beginning image retrieval from web");

        var request = Region.Projection.CreateMessage(this);
        if (request == null)
        {
            Logger.Error("Could not create HttpRequestMessage for mapTile ({0})", FragmentId);
            return null;
        }

        var uriText = request.RequestUri!.AbsoluteUri;
        var httpClient = new HttpClient();

        Logger.Verbose("Querying {0}", uriText);

        HttpResponseMessage? response;

        try
        {
            response = Region.Projection.MaxRequestLatency <= 0
                ? await httpClient.SendAsync(request, ctx)
                : await httpClient.SendAsync(request, ctx)
                                  .WaitAsync(TimeSpan.FromMilliseconds(Region.Projection.MaxRequestLatency),
                                              ctx);

            Logger.Verbose("Got response from {0}", uriText);
        }
        catch (Exception ex)
        {
            Logger.Error("Image request from {0} failed, message was '{1}'",
                           request.RequestUri,
                           ex.Message);
            return null;
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            Logger.Error( "Image request from {0} failed with response code {1}, message was '{2}'",
                           uriText,
                           response.StatusCode,
                           await response.Content.ReadAsStringAsync( ctx ) );

            return null;
        }

        Logger.Verbose("Reading response from {0}", uriText);

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

            return memStream.ToArray();
        }
        catch (Exception ex)
        {
            Logger.Error( "Could not retrieve bitmap image stream from {0}, message was '{1}'",
                           response.RequestMessage!.RequestUri!,
                           ex.Message );

            return null;
        }
    }

    public async Task<bool> LoadImageAsync( CancellationToken ctx = default )
    {
        ImageData = await GetImageAsync( ctx );
        return ImageData != null;
    }

    public async Task<bool> LoadFromCacheAsync( ITileCache? cache, CancellationToken ctx = default )
    {
        if( !InProjection )
        {
            ImageData = null;
            return true;
        }

        var retVal = false;

        if( cache != null )
            retVal = await cache.LoadImageAsync( this, ctx );

        if( retVal )
            return true;

        // load the image from the web, and then cache it if possible
        retVal = await LoadImageAsync( ctx );

        if ( retVal && cache != null )
            await cache.AddEntryAsync( this, ctx );

        return retVal;
    }
}
