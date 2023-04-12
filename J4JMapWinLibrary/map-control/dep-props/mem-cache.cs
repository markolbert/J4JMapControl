#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// mem-cache.cs
//
// This file is part of JumpForJoy Software's J4JMapWinLibrary.
// 
// J4JMapWinLibrary is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JMapWinLibrary is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JMapWinLibrary. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using J4JSoftware.J4JMapLibrary;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    public DependencyProperty UseMemoryCacheProperty = DependencyProperty.Register( nameof( UseMemoryCache ),
        typeof( bool ),
        typeof( J4JMapControl ),
        new PropertyMetadata( true ) );

    public bool UseMemoryCache
    {
        get => (bool) GetValue( UseMemoryCacheProperty );

        set
        {
            SetValue( UseMemoryCacheProperty, value );
            _logger?.LogTrace( "{status} memory cache", value ? "Enabling" : "Disabling" );

            UpdateCaching();
        }
    }

    public DependencyProperty MemoryCacheEntriesProperty = DependencyProperty.Register( nameof( MemoryCacheEntries ),
        typeof( int ),
        typeof( J4JMapControl ),
        new PropertyMetadata( DefaultMemoryCacheEntries ) );

    public int MemoryCacheEntries
    {
        get => (int) GetValue( MemoryCacheEntriesProperty );

        set
        {
            if( value <= 0 )
            {
                _logger?.LogWarning( "Invalid memory cache entries limit {limit}, defaulting to {default}",
                                     value,
                                     DefaultMemoryCacheEntries );

                value = DefaultMemoryCacheEntries;
            }

            SetValue( MemoryCacheEntriesProperty, value );
            _logger?.LogTrace("Memory cache to hold up to {entries} entries", value);

            UpdateCaching();
        }
    }

    public DependencyProperty MemoryCacheRetentionProperty = DependencyProperty.Register(
        nameof( MemoryCacheRetention ),
        typeof( string ),
        typeof( J4JMapControl ),
        new PropertyMetadata( DefaultMemoryCacheRetention.ToString() ) );

    public string MemoryCacheRetention
    {
        get => (string) GetValue( MemoryCacheRetentionProperty );

        set
        {
            if( !TimeSpan.TryParse( value, out var retention ) )
            {
                _logger?.LogWarning(
                    "Invalid memory cache retention period '{period}', defaulting to {default}",
                    value,
                    DefaultMemoryCacheRetention );

                value = DefaultMemoryCacheRetention.ToString();
            }

            SetValue( MemoryCacheRetentionProperty, value );
            _logger?.LogTrace("Memory cache to retain items for {retention}", value);

            UpdateCaching();
        }
    }

    public DependencyProperty MemoryCacheSizeProperty = DependencyProperty.Register( nameof( MemoryCacheSize ),
        typeof( int ),
        typeof( J4JMapControl ),
        new PropertyMetadata( DefaultMemoryCacheSize ) );

    public int MemoryCacheSize
    {
        get => (int) GetValue( MemoryCacheSizeProperty );

        set
        {
            if (value <= 0)
            {
                _logger?.LogWarning("Invalid memory cache size {size}, defaulting to {default}",
                                    value,
                                    DefaultMemoryCacheSize);

                value = DefaultMemoryCacheEntries;
            }

            SetValue( MemoryCacheSizeProperty, value );
            _logger?.LogTrace("Memory cache limited to {bytes} bytes", value);

            UpdateCaching();
        }
    }

    private void UpdateCaching()
    {
        if (_projection is not ITiledProjection tiledProjection)
            return;

        tiledProjection.TileCaching.RemoveAllCaches();

        // always add the memory cache first
        if (UseMemoryCache)
        {
            // should never happen, but...
            if (!TimeSpan.TryParse(MemoryCacheRetention, out var memRetention))
                memRetention = DefaultMemoryCacheRetention;

            var memCache = new MemoryCache("In Memory", LoggerFactory)
            {
                MaxBytes = MemoryCacheSize,
                MaxEntries = MemoryCacheEntries,
                RetentionPeriod = memRetention
            };

            _logger?.LogTrace( "Enabling memory cache, max bytes {bytes}, max entries {entries}, {retention} retention",
                               memCache.MaxBytes,
                               memCache.MaxEntries,
                               memCache.RetentionPeriod.ToString() );

            tiledProjection.TileCaching.AddCache(memCache);
        }

        if( string.IsNullOrEmpty( FileSystemCachePath ) )
            return;

        // should never happen, but...
        if (!TimeSpan.TryParse(FileSystemCacheRetention, out var fileRetention))
            fileRetention = TimeSpan.FromDays(1);

        var fileCache = new FileSystemCache("File System", LoggerFactory)
        {
            CacheDirectory = FileSystemCachePath,
            MaxBytes = FileSystemCacheSize,
            MaxEntries = FileSystemCacheEntries,
            RetentionPeriod = fileRetention
        };

        _logger?.LogTrace("Enabling file system cache, max bytes {bytes}, max entries {entries}, {retention} retention",
                          fileCache.MaxBytes,
                          fileCache.MaxEntries,
                          fileCache.RetentionPeriod.ToString());

        tiledProjection.TileCaching.AddCache(fileCache);
    }

    public void ClearCache( int level = -1 )
    {
        if( _projection is ITiledProjection tiledProjection )
            tiledProjection.TileCaching.Clear( level );
    }

    public void PurgeCache( int level = -1 )
    {
        if( _projection is ITiledProjection tiledProjection )
            tiledProjection.TileCaching.PurgeExpired( level );
    }

    public ReadOnlyCollection<CacheStats> GetCacheStats() =>
        _projection is ITiledProjection tiledProjection
            ? tiledProjection.TileCaching.CacheStats
            : new ReadOnlyCollection<CacheStats>( new List<CacheStats>() );
}
