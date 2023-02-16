using System.Runtime.CompilerServices;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract class StaticProjection<TAuth> : Projection<TAuth, INormalizedViewport, StaticFragment>
    where TAuth : class
{
    protected StaticProjection(
        IJ4JLogger logger
    )
        : base( logger )
    {
    }

    protected StaticProjection(
        IProjectionCredentials credentials,
        IJ4JLogger logger
    )
        : base( credentials, logger )
    {
    }

    public override async IAsyncEnumerable<StaticFragment> GetExtractAsync(
        INormalizedViewport viewportData,
        bool deferImageLoad = false,
        [EnumeratorCancellation] CancellationToken ctx = default
    )
    {
        if (!Initialized)
        {
            Logger.Error("Projection not initialized");
            yield break;
        }

        var mapTile = new StaticFragment(this,
                                          viewportData.CenterLatitude,
                                          viewportData.CenterLongitude,
                                          viewportData.Height,
                                          viewportData.Width,
                                          viewportData.Scale);

        if (!deferImageLoad)
            await mapTile.GetImageAsync(viewportData.Scale, ctx: ctx);

        yield return mapTile;
    }

}
