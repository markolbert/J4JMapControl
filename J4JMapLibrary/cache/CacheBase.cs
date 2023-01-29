using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract class CacheBase : ITileCache
{
    protected CacheBase(
        IJ4JLogger logger
    )
    {
        Logger = logger;
        Logger.SetLoggedType( GetType() );
    }

    protected IJ4JLogger Logger { get; }

    public ITileCache? ParentCache { get; set; }

    public abstract void Clear();
    public abstract void PurgeExpired();

    public virtual async Task<CacheEntry?> GetEntryAsync( ITiledProjection projection, int xTile, int yTile )
    {
        xTile = InternalExtensions.ConformValueToRange( xTile, projection.Metrics.TileXRange, "X Tile" );
        yTile = InternalExtensions.ConformValueToRange( yTile, projection.Metrics.TileYRange, "Y Tile" );

        var retVal = await GetEntryInternalAsync( projection, xTile, yTile );
        if( retVal != null )
        {
            retVal.LastAccessedUtc = DateTime.UtcNow;
            return retVal;
        }

        retVal = ParentCache == null ? null : await ParentCache.GetEntryAsync( projection, xTile, yTile );
        if( retVal == null )
            return await AddEntryAsync( projection, xTile, yTile );

        retVal.LastAccessedUtc = DateTime.UtcNow;
        return retVal;
    }

    protected abstract Task<CacheEntry?> GetEntryInternalAsync( ITiledProjection projection, int xTile, int yTile );
    protected abstract Task<CacheEntry?> AddEntryAsync( ITiledProjection projection, int xTile, int yTile );
}
