using System.Collections.ObjectModel;
using System.Formats.Tar;

namespace J4JMapLibrary;

public interface ITileCache
{
    void Clear();
    void PurgeExpired();
    Task<CacheEntry?> GetEntryAsync( ITiledProjection projection, int xTile, int yTile );
    ITileCache? ParentCache { get; }
    ReadOnlyCollection<string> QuadKeys { get; }
    int Count { get; }
}
