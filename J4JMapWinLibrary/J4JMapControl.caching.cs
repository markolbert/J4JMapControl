using System;
using J4JSoftware.J4JMapLibrary;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    public bool UseMemoryCache
    {
        get => (bool) GetValue( UseMemoryCacheProperty );
        set => SetValue( UseMemoryCacheProperty, value );
    }

    public int MemoryCacheSize
    {
        get => (int) GetValue( MemoryCacheSizeProperty );
        set => SetValue( MemoryCacheSizeProperty, value );
    }

    public int MemoryCacheEntries
    {
        get => (int) GetValue( MemoryCacheEntriesProperty );
        set => SetValue( MemoryCacheEntriesProperty, value );
    }

    public string MemoryCacheRetention
    {
        get => (string) GetValue( MemoryCacheRetentionProperty );
        set => SetValue( MemoryCacheRetentionProperty, value );
    }

    public string FileSystemCachePath
    {
        get => (string) GetValue( FileSystemCachePathProperty );
        set => SetValue( FileSystemCachePathProperty, value );
    }

    public int FileSystemCacheSize
    {
        get => (int) GetValue( FileSystemCacheSizeProperty );
        set => SetValue( FileSystemCacheSizeProperty, value );
    }

    public int FileSystemCacheEntries
    {
        get => (int) GetValue( FileSystemCacheEntriesProperty );
        set => SetValue( FileSystemCacheEntriesProperty, value );
    }

    public string FileSystemCacheRetention
    {
        get => (string) GetValue( FileSystemCacheRetentionProperty );
        set => SetValue( FileSystemCacheRetentionProperty, value );
    }

    private void UpdateCaching()
    {
        if( _cacheIsValid )
            return;

        if( !TimeSpan.TryParse( FileSystemCacheRetention, out var fileRetention ) )
            fileRetention = TimeSpan.FromDays( 1 );

        _tileFileCache = string.IsNullOrEmpty( FileSystemCachePath )
            ? null
            : new FileSystemCache( _logger )
            {
                CacheDirectory = FileSystemCachePath,
                MaxBytes = FileSystemCacheSize <= 0 ? DefaultFileSystemCacheSize : FileSystemCacheSize,
                MaxEntries = FileSystemCacheEntries <= 0 ? DefaultFileSystemCacheEntries : FileSystemCacheEntries,
                RetentionPeriod = fileRetention
            };

        if( !TimeSpan.TryParse( MemoryCacheRetention, out var memRetention ) )
            memRetention = TimeSpan.FromHours( 1 );

        _tileMemCache = UseMemoryCache
            ? new MemoryCache( _logger )
            {
                MaxBytes = MemoryCacheSize <= 0 ? DefaultMemoryCacheSize : MemoryCacheSize,
                MaxEntries = MemoryCacheEntries <= 0 ? DefaultMemoryCacheEntries : MemoryCacheEntries,
                ParentCache = _tileFileCache,
                RetentionPeriod = memRetention
            }
            : null;

        _cacheIsValid = true;
    }
}
