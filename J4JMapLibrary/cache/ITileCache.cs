using System.Collections.ObjectModel;

namespace J4JMapLibrary;

public interface ITileCache
{
    ITileCache? ParentCache { get; }
    ReadOnlyCollection<string> FragmentIds { get; }
    int Count { get; }
    void Clear();
    void PurgeExpired();

    Task<CacheEntry?> GetEntryAsync(
        ITiledProjection projection,
        int xTile,
        int yTile,
        int scale,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    );
}
