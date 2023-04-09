using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using J4JSoftware.J4JMapLibrary;
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
            SetValue( MemoryCacheEntriesProperty, value );
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
            SetValue( MemoryCacheRetentionProperty, value );
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
            SetValue( MemoryCacheSizeProperty, value );
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
            if (!TimeSpan.TryParse(MemoryCacheRetention, out var memRetention))
                memRetention = TimeSpan.FromHours(1);

            var memCache = new MemoryCache("In Memory", LoggerFactory)
            {
                MaxBytes =
                    MemoryCacheSize <= 0
                        ? DefaultMemoryCacheSize
                        : MemoryCacheSize,
                MaxEntries =
                    MemoryCacheEntries <= 0
                        ? DefaultMemoryCacheEntries
                        : MemoryCacheEntries,
                RetentionPeriod = memRetention
            };

            tiledProjection.TileCaching.AddCache(memCache);
        }

        if( string.IsNullOrEmpty( FileSystemCachePath ) )
            return;

        if (!TimeSpan.TryParse(FileSystemCacheRetention, out var fileRetention))
            fileRetention = TimeSpan.FromDays(1);

        var fileCache = new FileSystemCache("File System", LoggerFactory)
        {
            CacheDirectory = FileSystemCachePath,
            MaxBytes = FileSystemCacheSize <= 0 ? DefaultFileSystemCacheSize : FileSystemCacheSize,
            MaxEntries = FileSystemCacheEntries <= 0 ? DefaultFileSystemCacheEntries : FileSystemCacheEntries,
            RetentionPeriod = fileRetention
        };

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
