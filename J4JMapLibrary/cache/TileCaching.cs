#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// TileCaching.cs
//
// This file is part of JumpForJoy Software's J4JMapLibrary.
// 
// J4JMapLibrary is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JMapLibrary is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JMapLibrary. If not, see <https://www.gnu.org/licenses/>.
#endregion

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


    public async Task<int> LoadImageAsync(MapBlock mapBlock, CancellationToken ctx = default)
    {
        var levelFound = -1;

        foreach (var info in _infoCollection.OrderBy(x => x.Level))
        {
            if( !await info.Cache.LoadImageAsync( mapBlock, ctx ) )
                continue;

            levelFound = info.Level;
            break;
        }

        return levelFound;
    }

    public async Task UpdateCaches(MapBlock mapBlock, int foundLevel, CancellationToken ctx = default)
    {
        foreach( var info in _infoCollection.Where( x => x.Level < foundLevel ) )
        {
            await info.Cache.AddEntryAsync( mapBlock, ctx );
        }
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
