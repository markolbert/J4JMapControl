using System.Formats.Tar;

namespace J4JMapLibrary;

public interface ITileCache
{
    void Clear();
    void PurgeExpired();
    CacheEntry? GetEntry( ITiledProjection projection, int xTile, int yTile );
    ITileCache? ParentCache { get; }
}

//public interface ITileCache<out TEntry> : ITileCache
//    where TEntry : class, ICacheEntry
//{
//    ITileCache<TEntry>? ParentCache { get; }
//    TEntry? GetCachedEntry( ITiledProjection projection, int xTile, int yTile);
//}