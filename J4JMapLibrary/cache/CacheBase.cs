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

    public int MaxEntries { get; set; }
    public long MaxBytes { get; set; }
    public TimeSpan RetentionPeriod { get; set; } = TimeSpan.Zero;

    public ITileCache? ParentCache { get; set; }
    public abstract int Count { get; }
    public abstract ReadOnlyCollection<string> FragmentIds { get; }

    public abstract void Clear();
    public abstract void PurgeExpired();

    public virtual async Task<CacheEntry?> GetEntryAsync(
        ITiledProjection projection,
        int xTile,
        int yTile,
        int scale,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    )
    {
        xTile = projection.TileXRange.ConformValueToRange( xTile, "X Tile" );
        yTile = projection.TileYRange.ConformValueToRange( yTile, "Y Tile" );

        var retVal = await GetEntryInternalAsync( projection, xTile, yTile, scale, deferImageLoad, ctx );
        if( retVal != null )
        {
            retVal.LastAccessedUtc = DateTime.UtcNow;
            return retVal;
        }

        retVal = ParentCache == null
            ? null
            : await ParentCache.GetEntryAsync( projection, xTile, yTile, scale, deferImageLoad, ctx );

        retVal ??= await AddEntryAsync( projection, xTile, yTile, scale, deferImageLoad, ctx );

        if( retVal == null )
        {
            Logger.Error( "Failed to create {0} cache entry for mapFragment ({1}, {2})",
                          projection.Name,
                          xTile,
                          yTile );

            return null;
        }

        retVal.LastAccessedUtc = DateTime.UtcNow;
        return retVal;
    }

    protected abstract Task<CacheEntry?> GetEntryInternalAsync(
        ITiledProjection projection,
        int xTile,
        int yTile,
        int scale,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    );

    protected abstract Task<CacheEntry?> AddEntryAsync(
        ITiledProjection projection,
        int xTile,
        int yTile,
        int scale,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    );
}
