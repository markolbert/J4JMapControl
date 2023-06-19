using System.Net;
using J4JSoftware.VisualUtilities;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapLibrary;

public abstract partial class Projection
{
    public float? GetRegionZoom(Region requestedRegion)
    {
        if (requestedRegion.CenterPoint == null)
            return null;

        var heightWidth = GetHeightWidth(requestedRegion.Scale);
        var projRectangle = new Rectangle2D(heightWidth, heightWidth, coordinateSystem: CoordinateSystem2D.Display);

        var shrinkResult = projRectangle.ShrinkToFit(requestedRegion.Area!, requestedRegion.ShrinkStyle);

        return shrinkResult.Zoom;
    }

    public abstract Task<MapBlock?> GetMapTileAsync(
        int x,
        int y,
        int scale,
        CancellationToken ctx = default
    );

    public async Task<byte[]?> GetImageAsync(MapBlock mapBlock, CancellationToken ctx = default)
    {
        Logger?.LogTrace("Beginning image retrieval from web");

        var request = CreateMessage(mapBlock);
        if (request == null)
        {
            Logger?.LogError("Could not create HttpRequestMessage for mapBlock ({fragmentId})", mapBlock.FragmentId);
            return null;
        }

        var uriText = request.RequestUri!.AbsoluteUri;
        var httpClient = new HttpClient();

        Logger?.LogTrace("Querying {uriText}", uriText);

        HttpResponseMessage? response;

        try
        {
            response = MaxRequestLatency <= 0
                ? await httpClient.SendAsync(request, ctx)
                : await httpClient.SendAsync(request, ctx)
                                  .WaitAsync(TimeSpan.FromMilliseconds(MaxRequestLatency),
                                              ctx);

            Logger?.LogTrace("Got response from {uriText}", uriText);
        }
        catch (Exception ex)
        {
            Logger?.LogError("Image request from {uri} failed, message was '{errorMesg}'",
                              request.RequestUri,
                              ex.Message);
            return null;
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            Logger?.LogError("Image request from {uri} failed with response code {respCode}, message was '{mesg}'",
                              uriText,
                              response.StatusCode,
                              await response.Content.ReadAsStringAsync(ctx));

            return null;
        }

        Logger?.LogTrace("Reading response from {uri}", uriText);

        // extract image data from response
        try
        {
            await using var responseStream = MaxRequestLatency < 0
                ? await response.Content.ReadAsStreamAsync(ctx)
                : await response.Content.ReadAsStreamAsync(ctx)
                                .WaitAsync(TimeSpan.FromMilliseconds(MaxRequestLatency),
                                            ctx);

            var memStream = new MemoryStream();
            await responseStream.CopyToAsync(memStream, ctx);

            return memStream.ToArray();
        }
        catch (Exception ex)
        {
            Logger?.LogError("Could not retrieve bitmap image stream from {uri}, message was '{mesg}'",
                              response.RequestMessage!.RequestUri!,
                              ex.Message);

            return null;
        }
    }

    protected abstract HttpRequestMessage? CreateMessage(MapBlock mapBlock);

    public virtual async Task<bool> LoadImageAsync(MapBlock mapBlock, CancellationToken ctx = default)
    {
        mapBlock.ImageData = await GetImageAsync(mapBlock, ctx);
        return mapBlock.ImageData != null;
    }

    public abstract Task<IMapRegion?> LoadRegionAsync(
        Region region,
        CancellationToken ctx = default( CancellationToken )
    );

    protected virtual void OnRegionProcessed(bool loaded) => RegionProcessed?.Invoke(this, loaded);

    public async Task<bool> LoadBlocksAsync(IEnumerable<MapBlock> blocks, CancellationToken ctx = default)
    {
        if (!Initialized)
        {
            Logger?.LogError("Projection not initialized");
            return false;
        }

        var retVal = await LoadBlocksInternalAsync(blocks, ctx);

        return retVal;
    }

    protected abstract Task<bool> LoadBlocksInternalAsync(
        IEnumerable<MapBlock> blocks,
        CancellationToken ctx = default
    );
}
