using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract class StaticProjection<TAuth> : Projection<TAuth>, IStaticProjection
    where TAuth : class
{
    protected StaticProjection(
        IMapServer mapServer,
        ProjectionScale projectionScale,
        IJ4JLogger logger
    )
        : base( mapServer, projectionScale, logger )
    {
    }

    protected StaticProjection(
        IProjectionCredentials credentials,
        IMapServer mapServer,
        ProjectionScale projectionScale,
        IJ4JLogger logger
    )
        : base( credentials, mapServer, projectionScale, logger )
    {
    }

    public async Task<List<IStaticFragment>?> GetViewportRegionAsync(
        NormalizedViewport viewportData,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    )
    {
        var extract = await GetExtractAsync(viewportData, deferImageLoad, ctx);

        if (extract == null)
            return null;

        return await extract.GetTilesAsync(viewportData.Scale, ctx)
                            .ToListAsync(ctx);
    }

    public async Task<StaticExtract?> GetExtractAsync(
        INormalizedViewport viewportData,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    )
    {
        if (!Initialized)
        {
            Logger.Error("Projection not initialized");
            return null;
        }

        var mapTile = new StaticFragment( this,
                                          viewportData.CenterLatitude,
                                          viewportData.CenterLongitude,
                                          viewportData.Height,
                                          viewportData.Width,
                                          viewportData.Scale );

        if (!deferImageLoad)
            await mapTile.GetImageAsync(viewportData.Scale, ctx: ctx);

        var retVal = new StaticExtract(this, Logger);

        if (!retVal.Add(mapTile))
            Logger.Error("Problem adding StaticFragment to collection (probably differing ITiledScale)");

        return retVal;
    }

}
