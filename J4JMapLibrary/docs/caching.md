# J4JMapLibary: Caching

Some of the projections support caching. For the ones that do, it's strongly recommended you enable it because it improves performance by cutting down on the number of web requests for map images.

Projections supporting caching have a `TileCaching` property that returns an instance of `ITileCaching`.

The `ITileCaching` interface looks like this:

```csharp
public interface ITileCaching 
{
    ReadOnlyCollection<CacheInfo> Caches { get; }
    ReadOnlyCollection<CacheStats> CacheStats { get; }

    void Clear( int minLevel = -1 );
    void PurgeExpired( int minLevel = -1 );

    bool AddCache( ITileCache tileCache );
    bool RemoveCache( ITileCache tileCache );
    bool RemoveCache( string name );
    bool RemoveCache( int level );
    void RemoveAllCaches();

    Task<int> LoadImageAsync(MapTile mapTile, CancellationToken ctx = default);
    Task UpdateCaches( MapTile mapTile, int foundLevel, CancellationToken ctx = default );
}
```

`ITileCaching` organizes whatever caches you specify, keeping them all synchronized as the caches change. *The order in which you add caches to `ITileCaching` is important*: each added `ITileCache` instance is deemed to be *behind* the prior caches in the list.

For example, to have a `FileSystemCache` that works behind a `MemoryCache` you would add the memory cache instance first (details omitted):

```csharp
cachableProjection.TileCaching.Add( new MemoryCache( ... ) );
cachableProjection.TileCaching.Add( new FileSystemCache(...) );
```

If an earlier cache can satisfy a request the later caches are not accessed.

The library comes with two caching classes: `MemoryCache` and `FileSystemCache`. Here are their constructors:

```csharp
public MemoryCache( string name, ILoggerFactory? logger = null )

public FileSystemCache( string name, ILoggerFactory? loggerFactory = null )
```

Each cache should have a unique name. If you don't specify an instance of `ILoggerFactory` you won't get logging events.

Both `MemoryCache` and `FileSystemCache` are derived from `CacheBase`. They each offer a common set of configuration properties:

|Property|Type|Comment|
|--------|----|-------|
|`MaxEntries`|`int`|the maximum number of entries the cache will hold. When exceeded, a purge cycle is initiated|
|`MaxBytes`|`long`|the maximum number of bytes the cache will hold. When exceeded, a purge cycle is initiated|
|`RetentionPeriod`|`TimeSpan`|entires older than `RetentionPeriod` will be purged during a purge cycle|

[return to usage](usage.md)
