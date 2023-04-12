using System.Collections.ObjectModel;
using J4JSoftware.J4JMapLibrary.MapRegion;

namespace J4JSoftware.J4JMapLibrary;

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
