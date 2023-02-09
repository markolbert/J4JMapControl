using System.Collections.ObjectModel;
using System.Formats.Tar;

namespace J4JMapLibrary;

public interface ITileCache
{
    void Clear();
    void PurgeExpired();

    Task<CacheEntry?> GetEntryAsync(
        IFixedTileProjection projection,
        int xTile,
        int yTile,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    );

    ITileCache? ParentCache { get; }
    ReadOnlyCollection<string> QuadKeys { get; }
    int Count { get; }
}
