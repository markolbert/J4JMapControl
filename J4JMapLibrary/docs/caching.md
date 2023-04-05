# J4JMapLibary: Caching

The `ITileCache` interface looks like this:

```csharp
public interface ITileCache : IEnumerable<CachedEntry>
{
    ITileCache? ParentCache { get; }
    CacheStats Stats { get; }

    void Clear();
    void PurgeExpired();

    Task<bool> LoadImageAsync( MapTile mapTile, CancellationToken ctx = default );
    Task<bool> AddEntryAsync( MapTile mapTile, CancellationToken ctx = default );
}
```

The caching system is designed to work in a tiered fashion, with each *level* of caching optinally calling upon a parent cache if it itself cannot satisfy the request for a map image.

The library comes with two caching classes: `MemoryCache` and `FileSystemCache`. What they do is pretty self-explanatory. Refer to the [caching documentation](caching.md) for more details on how to design your own cache.

You'd typically set up a multi-level caching system like this:

```csharp
var tileFileCache = new FileSystemCache(LoggerFactory)
    {
        CacheDirectory = FileSystemCachePath,
        MaxBytes = FileSystemCacheSize <= 0 ? DefaultFileSystemCacheSize : FileSystemCacheSize,
        MaxEntries = FileSystemCacheEntries <= 0 ? DefaultFileSystemCacheEntries : FileSystemCacheEntries,
        RetentionPeriod = fileRetention
    };

if (!TimeSpan.TryParse(MemoryCacheRetention, out var memRetention))
    memRetention = TimeSpan.FromHours(1);

var tileMemCache = new MemoryCache(LoggerFactory)
    {
        MaxBytes = MemoryCacheSize <= 0 ? DefaultMemoryCacheSize : MemoryCacheSize,
        MaxEntries = MemoryCacheEntries <= 0 ? DefaultMemoryCacheEntries : MemoryCacheEntries,
        ParentCache = tileFileCache,
        RetentionPeriod = memRetention
    };
```
