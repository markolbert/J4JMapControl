#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// MemoryCache.cs
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

using J4JSoftware.J4JMapLibrary.MapRegion;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapLibrary;

public class MemoryCache : CacheBase
{
    private readonly Dictionary<string, CachedTile> _cached = new( StringComparer.OrdinalIgnoreCase );

    public MemoryCache(
        string name,
        ILoggerFactory? logger = null
    )
        : base( name, logger )
    {
    }

    public override void Clear()
    {
        _cached.Clear();
        Stats.Reload( this );
    }

    public override void PurgeExpired()
    {
        var toDelete = _cached
                      .Where( x => RetentionPeriod != TimeSpan.Zero
                               && x.Value.LastAccessedUtc < DateTime.UtcNow - RetentionPeriod )
                      .Select( x => x.Key )
                      .ToList();

        foreach( var key in toDelete )
        {
            _cached.Remove( key );
        }

        if( MaxEntries > 0 && _cached.Count > MaxEntries )
        {
            var entriesByOldest = _cached.OrderBy( x => x.Value.CreatedUtc )
                                         .ToList();

            while( entriesByOldest.Count > MaxEntries )
            {
                _cached.Remove( entriesByOldest.First().Key );
                entriesByOldest.RemoveAt( 0 );
            }
        }

        if( MaxBytes <= 0 || _cached.Sum( x => x.Value.Tile.ImageBytes ) <= MaxBytes )
        {
            Stats.Reload( this );
            return;
        }

        var entriesByLargest = _cached.OrderByDescending( x => x.Value.Tile.ImageBytes )
                                      .ToList();

        while( MaxBytes >= 0 && entriesByLargest.Sum( x => x.Value.Tile.ImageBytes ) > MaxBytes )
        {
            _cached.Remove( entriesByLargest.First().Key );
            entriesByLargest.RemoveAt( 0 );
        }

        Stats.Reload( this );
    }

#pragma warning disable CS1998
    protected override async Task<bool> LoadImageDataInternalAsync(
#pragma warning restore CS1998
        MapTile mapTile,
        CancellationToken ctx = default
    )
    {
        // shouldn't ever happen, but...
        if( string.IsNullOrEmpty( mapTile.QuadKey ) )
            return false;

        if( !_cached.TryGetValue( mapTile.FragmentId, out var cachedTile ) )
            return false;

        mapTile.ImageData = cachedTile.Tile.ImageData;

        return mapTile.ImageData != null;
    }

#pragma warning disable CS1998
    public override async Task<bool> AddEntryAsync( MapTile mapTile, CancellationToken ctx = default )
#pragma warning restore CS1998
    {
        if( mapTile.ImageData == null )
        {
            Logger?.LogError("Map tile contains no image data");
            return false;
        }

        var cacheEntry = new CachedTile( mapTile.ImageBytes, DateTime.UtcNow, DateTime.UtcNow, mapTile );

        if( _cached.ContainsKey( mapTile.FragmentId ) )
        {
            Logger?.LogWarning( "Replacing memory cache entry '{fragmentId}'", cacheEntry.Tile.FragmentId );
            _cached[ mapTile.FragmentId ] = cacheEntry;
        }
        else
        {
            _cached.Add( mapTile.FragmentId, cacheEntry );
            Logger?.LogTrace("Added memory cache entry '{fragmentId}'", cacheEntry.Tile.FragmentId);
        }

        Stats.RecordEntry(mapTile.ImageData);

        if ( ( MaxEntries > 0 && Stats.Entries > MaxEntries ) || ( MaxBytes > 0 && Stats.Bytes > MaxBytes ) )
            PurgeExpired();

        return true;
    }

    public override IEnumerator<CachedEntry> GetEnumerator()
    {
        foreach( var kvp in _cached )
        {
            yield return kvp.Value;
        }
    }
}
