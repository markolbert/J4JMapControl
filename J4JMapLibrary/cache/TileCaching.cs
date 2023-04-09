using System.Collections.ObjectModel;
using J4JSoftware.J4JMapLibrary.MapRegion;

namespace J4JSoftware.J4JMapLibrary;

public class TileCaching : ITileCaching
{
    private readonly List<CacheInfo> _infoCollection = new();

    public ReadOnlyCollection<CacheStats> CacheStats =>
        _infoCollection.Select( x => x.Cache.Stats ).ToList().AsReadOnly();

    public void Clear( int minLevel = -1 )
    {
        foreach( var cacheInfo in _infoCollection.Where( x => x.Level >= minLevel ) )
        {
            cacheInfo.Cache.Clear();
        }
    }

    public void PurgeExpired( int minLevel = -1 )
    {
        foreach( var cacheInfo in _infoCollection.Where( x => x.Level >= minLevel ) )
        {
            cacheInfo.Cache.PurgeExpired();
        }
    }


    public async Task<bool> LoadImageAsync(MapTile mapTile, CancellationToken ctx = default)
    {
        var levelFound = -1;

        foreach (var info in _infoCollection.OrderBy(x => x.Level))
        {
            if( !await info.Cache.LoadImageAsync( mapTile, ctx ) )
                continue;

            levelFound = info.Level;
            break;
        }

        return await AddEntryAsync( mapTile, levelFound, ctx );
    }

    private async Task<bool> AddEntryAsync(MapTile mapTile, int levelFound, CancellationToken ctx = default)
    {
        var allAdded = true;

        foreach( var info in _infoCollection.Where( x => x.Level != levelFound ) )
        {
            allAdded &= await info.Cache.AddEntryAsync( mapTile, ctx );
        }

        return allAdded;
    }

    public ReadOnlyCollection<CacheInfo> Caches => _infoCollection.AsReadOnly();

    public bool AddCache( ITileCache tileCache )
    {
        var nextLevel = _infoCollection.Any() ? _infoCollection.Max( x => x.Level ) + 1 : 0;
        _infoCollection.Add( new CacheInfo( nextLevel, tileCache ) );

        return true;
    }

    public bool RemoveCache( ITileCache tileCache )
    {
        var info = _infoCollection.FirstOrDefault( x => ReferenceEquals( tileCache, x.Cache ) );
        if( info == null )
            return false;

        _infoCollection.Remove( info );
        return true;
    }

    public bool RemoveCache( string name )
    {
        var info = _infoCollection.FirstOrDefault( x => x.Name.Equals( name, StringComparison.OrdinalIgnoreCase ) );
        if( info == null )
            return false;

        _infoCollection.Remove( info );
        return true;
    }

    public bool RemoveCache( int level )
    {
        var info = _infoCollection.FirstOrDefault(x => x.Level == level);
        if (info == null)
            return false;

        _infoCollection.Remove(info);
        return true;
    }

    public void RemoveAllCaches() => _infoCollection.Clear();
}
