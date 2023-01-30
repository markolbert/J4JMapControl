using System.Collections.ObjectModel;
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
    public abstract int Count { get; }
    public abstract ReadOnlyCollection<string> QuadKeys { get; }

    public int MaxEntries { get; set; }
    public long MaxBytes { get; set; }
    public TimeSpan RetentionPeriod { get; set; } = TimeSpan.Zero;

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
        retVal ??= await AddEntryAsync( projection, xTile, yTile );

        if( retVal == null )
        {
            Logger.Error( "Failed to create {0} cache entry for tile ({1}, {2})",
                          projection.Name,
                          xTile,
                          yTile );

            return null;
        }

        retVal.LastAccessedUtc = DateTime.UtcNow;
        return retVal;
    }

    protected abstract Task<CacheEntry?> GetEntryInternalAsync( ITiledProjection projection, int xTile, int yTile );
    protected abstract Task<CacheEntry?> AddEntryAsync( ITiledProjection projection, int xTile, int yTile );
}
