#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// ITileCaching.cs
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
