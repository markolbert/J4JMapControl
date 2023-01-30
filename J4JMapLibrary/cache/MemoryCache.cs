using System.Collections.ObjectModel;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public class MemoryCache : CacheBase
{
    private readonly Dictionary<string, CacheEntry> _cached = new(StringComparer.OrdinalIgnoreCase);

    public MemoryCache(
        IJ4JLogger logger
    )
        : base(logger)
    {
    }

    public override int Count => _cached.Count;

    public override ReadOnlyCollection<string> QuadKeys => _cached
        .Select(x => x.Value.Tile.QuadKey)
        .ToList()
        .AsReadOnly();

    public override void Clear() => _cached.Clear();

    public override void PurgeExpired()
    {
        var toDelete = _cached
                      .Where( x => RetentionPeriod != TimeSpan.Zero
                               && ( x.Value.LastAccessedUtc < DateTime.UtcNow - RetentionPeriod ) )
                      .Select( x => x.Key )
                      .ToList();

        foreach( var key in toDelete )
        {
            _cached.Remove( key );
        }

        if( MaxEntries > 0 && _cached.Count > MaxEntries )
        {
            var entriesByOldest = _cached.OrderBy( x => x.Value.CreatedUtc )
                                         .ToList();

            while( entriesByOldest.Count > MaxEntries )
            {
                _cached.Remove( entriesByOldest.First().Key );
                entriesByOldest.RemoveAt( 0 );
            }
        }

        if( MaxBytes <= 0 || _cached.Sum( x => x.Value.Tile.ImageBytes ) <= MaxBytes )
            return;

        var entriesByLargest = _cached.OrderByDescending( x => x.Value.Tile.ImageBytes )
                                      .ToList();

        while( MaxBytes >= 0 && entriesByLargest.Sum( x => x.Value.Tile.ImageBytes ) > MaxBytes )
        {
            _cached.Remove( entriesByLargest.First().Key );
            entriesByLargest.RemoveAt( 0 );
        }
    }

#pragma warning disable CS1998
    protected override async Task<CacheEntry?> GetEntryInternalAsync(ITiledProjection projection, int xTile, int yTile)
#pragma warning restore CS1998
    {
        var quadKey = projection.GetQuadKey( xTile, yTile );
        if( string.IsNullOrEmpty( quadKey ) )
            return null;

        var key = $"{projection.Name}-{quadKey}";

        return _cached.ContainsKey(key) ? _cached[key] : null;
    }

    protected override async Task<CacheEntry?> AddEntryAsync(ITiledProjection projection, int xTile, int yTile)
    {
        var quadKey = projection.GetQuadKey(xTile, yTile);
        if (string.IsNullOrEmpty(quadKey))
            return null;

        var retVal = new CacheEntry(projection, xTile, yTile);

        var imageData = await retVal.Tile.GetImageAsync() ?? Array.Empty<byte>();

        if ( imageData.Length == 0 )
        {
            Logger.Error("Failed to retrieve image data");
            return null;
        }

        var key = $"{projection.Name}-{quadKey}";

        if (_cached.ContainsKey(key))
        {
            Logger.Warning<string>("Replacing map tile with quadkey '{0}'", retVal.Tile.QuadKey);
            _cached[key] = retVal;
        }
        else
        {
            _cached.Add(key, retVal);

            if( MaxEntries > 0 && _cached.Count > MaxEntries
            || MaxBytes > 0 && _cached.Sum( x => x.Value.Tile.ImageBytes ) > MaxBytes )
                PurgeExpired();
        }

        return retVal;
    }
}
