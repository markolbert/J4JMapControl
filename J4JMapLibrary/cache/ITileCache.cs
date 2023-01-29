using System.Formats.Tar;

namespace J4JMapLibrary;

public interface ITileCache
{
    void Clear();
    void PurgeExpired();
    CacheEntry? GetEntry( ITiledProjection projection, int xTile, int yTile );
    ITileCache? ParentCache { get; }
}
