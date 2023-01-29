using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract class CacheBase<TEntry> : ITileCache<TEntry>
    where TEntry : class, ICacheEntry
{
    protected CacheBase(
        IJ4JLogger logger,
        ITileCache<TEntry>? parentCache = null
    )
    {
        ParentCache = parentCache;

        Logger = logger;
        Logger.SetLoggedType( GetType() );
    }

    protected IJ4JLogger Logger { get; }

    public ITileCache<TEntry>? ParentCache { get; }

    public abstract void Clear();
    public abstract void PurgeExpired();

    public virtual TEntry? GetCachedEntry( ITiledProjection projection, int xTile, int yTile )
    {
        xTile = InternalExtensions.ConformValueToRange( xTile, projection.Metrics.TileXRange, "X Tile" );
        yTile = InternalExtensions.ConformValueToRange( yTile, projection.Metrics.TileYRange, "Y Tile" );

        var retVal = GetEntryInternal( projection, xTile, yTile );
        if( retVal != null )
        {
            retVal.LastAccessedUtc = DateTime.UtcNow;
            return retVal;
        }

        retVal = ParentCache?.GetCachedEntry( projection, xTile, yTile );
        if (retVal != null)
        {
            retVal.LastAccessedUtc = DateTime.UtcNow;
            return retVal;
        }

        try
        {
            retVal = (TEntry?) Activator.CreateInstance( typeof( TEntry ), new object?[] { projection, xTile, yTile } );

            if( retVal != null )
            {
                AddEntry( projection.Name, retVal );
                return retVal;
            }

            Logger.Error( "Could not create {0} cache entry for ({1},{2})", typeof( TEntry ), xTile, yTile );
        }
        catch( Exception ex )
        {
            Logger.Error( "Could not create {0} cache entry for ({1},{2}), message was {3}",
                          new object[] { typeof( TEntry ), xTile, yTile, ex.Message } );
        }

        return null;
    }

    protected abstract TEntry? GetEntryInternal( ITiledProjection projection, int xTile, int yTile );
    protected abstract bool AddEntry( string projectionName, TEntry entry );

    ICacheEntry? ITileCache.GetEntry( ITiledProjection projection, int xTile, int yTile ) =>
        GetCachedEntry( projection, xTile, yTile );
}
