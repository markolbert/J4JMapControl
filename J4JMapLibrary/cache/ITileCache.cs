using System.Collections.ObjectModel;

namespace J4JMapLibrary;

public interface ITileCache
{
    ITileCache? ParentCache { get; }
    ReadOnlyCollection<string> QuadKeys { get; }
    int Count { get; }
    void Clear();
    void PurgeExpired();

    Task<CacheEntry?> GetEntryAsync(
        IFixedTileProjection projection,
        int xTile,
        int yTile,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    );
}
