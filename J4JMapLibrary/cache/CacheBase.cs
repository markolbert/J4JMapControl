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
    public abstract ReadOnlyCollection<string> QuadKeys { get; }

    public abstract void Clear();
    public abstract void PurgeExpired();

    public virtual async Task<CacheEntry?> GetEntryAsync(
        IFixedTileProjection projection,
        int xTile,
        int yTile,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    )
    {
        xTile = projection.TileXRange.ConformValueToRange( xTile, "X Tile" );
        yTile = projection.TileYRange.ConformValueToRange( yTile, "Y Tile" );

        var retVal = await GetEntryInternalAsync( projection, xTile, yTile, deferImageLoad, ctx );
        if( retVal != null )
        {
            retVal.LastAccessedUtc = DateTime.UtcNow;
            return retVal;
        }

        retVal = ParentCache == null
            ? null
            : await ParentCache.GetEntryAsync( projection, xTile, yTile, deferImageLoad, ctx );

        retVal ??= await AddEntryAsync( projection, xTile, yTile, deferImageLoad, ctx );

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

    protected abstract Task<CacheEntry?> GetEntryInternalAsync(
        IFixedTileProjection projection,
        int xTile,
        int yTile,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    );

    protected abstract Task<CacheEntry?> AddEntryAsync(
        IFixedTileProjection projection,
        int xTile,
        int yTile,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    );
}
