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

    public virtual CacheEntry? GetEntry( ITiledProjection projection, int xTile, int yTile )
    {
        xTile = InternalExtensions.ConformValueToRange( xTile, projection.Metrics.TileXRange, "X Tile" );
        yTile = InternalExtensions.ConformValueToRange( yTile, projection.Metrics.TileYRange, "Y Tile" );

        var retVal = GetEntryInternal( projection, xTile, yTile );
        if( retVal != null )
        {
            retVal.LastAccessedUtc = DateTime.UtcNow;
            return retVal;
        }

        retVal = ParentCache?.GetEntry( projection, xTile, yTile );
        if( retVal == null )
            return AddEntry( projection, xTile, yTile );

        retVal.LastAccessedUtc = DateTime.UtcNow;
        return retVal;
    }

    protected abstract CacheEntry? GetEntryInternal( ITiledProjection projection, int xTile, int yTile );
    protected abstract CacheEntry? AddEntry( ITiledProjection projection, int xTile, int yTile );
}
