using J4JSoftware.Logging;

namespace J4JMapLibrary;

public class MemoryCache<TEntry> : CacheBase<TEntry>
    where TEntry : class, ICacheEntry
{
    private readonly Dictionary<string, TEntry> _cached = new(StringComparer.OrdinalIgnoreCase);

    public MemoryCache( 
        IJ4JLogger logger, 
        ITileCache<TEntry>? parentCache = null 
    )
        : base( logger, parentCache )
    {
    }

    public int MaxEntries { get; set; }
    public TimeSpan RetentionPeriod { get; set; } = TimeSpan.Zero;

    public override void Clear() => _cached.Clear();

    public override void PurgeExpired()
    {
        if( RetentionPeriod == TimeSpan.Zero ) 
            return;

        var toDelete = _cached.Where( x => x.Value.LastAccessedUtc < DateTime.UtcNow - RetentionPeriod )
                              .Select( x => x.Key )
                              .ToList();

        foreach( var key in toDelete )
        {
            _cached.Remove( key );
        }
    }

    protected override TEntry? GetEntryInternal( ITiledProjection projection, int xTile, int yTile )
    {
        var key = $"{projection.Name}{projection.GetQuadKey( xTile, yTile )}";

        return _cached.ContainsKey(key) ? _cached[key] : null;
    }

    protected override bool AddEntry( string projectionName, TEntry entry )
    {
        var key = $"{projectionName}{entry.Tile.QuadKey}";

        if( _cached.ContainsKey( key ) )
        {
            Logger.Warning<string>( "Replacing map tile with quadkey '{0}'", entry.Tile.QuadKey );
            _cached[ key ] = entry;
        }
        else
        {
            _cached.Add( key, entry );

            if( MaxEntries > 0 && _cached.Count > MaxEntries )
                PurgeExpired();
        }

        return true;
    }
}
