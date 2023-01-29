using J4JSoftware.Logging;

namespace J4JMapLibrary;

public class MemoryCache : CacheBase
{
    private readonly Dictionary<string, CacheEntry> _cached = new(StringComparer.OrdinalIgnoreCase);

    public MemoryCache( 
        IJ4JLogger logger
    )
        : base( logger )
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

#pragma warning disable CS1998
    protected override async Task<CacheEntry?> GetEntryInternalAsync( ITiledProjection projection, int xTile, int yTile )
#pragma warning restore CS1998
    {
        var key = $"{projection.Name}{projection.GetQuadKeyAsync( xTile, yTile )}";

        return _cached.ContainsKey(key) ? _cached[key] : null;
    }

#pragma warning disable CS1998
    protected override async Task<CacheEntry?> AddEntryAsync( ITiledProjection projection, int xTile, int yTile )
#pragma warning restore CS1998
    {
        var retVal = new CacheEntry( projection, xTile, yTile );

        var key = $"{projection.Name}{retVal.Tile.QuadKey}";

        if( _cached.ContainsKey( key ) )
        {
            Logger.Warning<string>( "Replacing map tile with quadkey '{0}'", retVal.Tile.QuadKey );
            _cached[ key ] = retVal;
        }
        else
        {
            _cached.Add( key, retVal );

            if( MaxEntries > 0 && _cached.Count > MaxEntries )
                PurgeExpired();
        }

        return retVal;
    }
}
