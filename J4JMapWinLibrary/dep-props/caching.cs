using System;
using J4JSoftware.J4JMapLibrary;
using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    public DependencyProperty UseMemoryCacheProperty = DependencyProperty.Register(nameof(UseMemoryCache),
        typeof(bool),
        typeof(J4JMapControl),
        new PropertyMetadata(true, OnCachingChanged));

    public DependencyProperty MemoryCacheSizeProperty = DependencyProperty.Register(nameof(MemoryCacheSize),
        typeof(int),
        typeof(J4JMapControl),
        new PropertyMetadata(DefaultMemoryCacheSize, OnCachingChanged));

    public DependencyProperty MemoryCacheEntriesProperty = DependencyProperty.Register(nameof(MemoryCacheEntries),
        typeof(int),
        typeof(J4JMapControl),
        new PropertyMetadata(DefaultMemoryCacheEntries, OnCachingChanged));

    public DependencyProperty MemoryCacheRetentionProperty = DependencyProperty.Register(
        nameof(MemoryCacheRetention),
        typeof(string),
        typeof(J4JMapControl),
        new PropertyMetadata(DefaultMemoryCacheRetention.ToString(), OnCachingChanged));

    public DependencyProperty FileSystemCachePathProperty = DependencyProperty.Register(nameof(FileSystemCachePath),
        typeof(string),
        typeof(J4JMapControl),
        new PropertyMetadata(GetDefaultFileSystemCachePath(), OnCachingChanged));

    public DependencyProperty FileSystemCacheSizeProperty = DependencyProperty.Register(nameof(FileSystemCacheSize),
        typeof(int),
        typeof(J4JMapControl),
        new PropertyMetadata(DefaultFileSystemCacheSize, OnCachingChanged));

    public DependencyProperty FileSystemCacheEntriesProperty = DependencyProperty.Register(
        nameof(FileSystemCacheEntries),
        typeof(int),
        typeof(J4JMapControl),
        new PropertyMetadata(DefaultFileSystemCacheEntries, OnCachingChanged));

    public DependencyProperty FileSystemCacheRetentionProperty = DependencyProperty.Register(
        nameof(FileSystemCacheRetention),
        typeof(string),
        typeof(J4JMapControl),
        new PropertyMetadata(DefaultFileSystemCacheRetention.ToString(), OnCachingChanged));

    private static void OnCachingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not J4JMapControl mapControl)
            return;

        mapControl._cacheIsValid = false;
        mapControl.UpdateCaching();
    }

    private void UpdateCaching()
    {
        if (_cacheIsValid)
            return;

        if (!TimeSpan.TryParse(FileSystemCacheRetention, out var fileRetention))
            fileRetention = TimeSpan.FromDays(1);

        _tileFileCache = string.IsNullOrEmpty(FileSystemCachePath)
            ? null
            : new FileSystemCache(_logger)
            {
                CacheDirectory = FileSystemCachePath,
                MaxBytes = FileSystemCacheSize <= 0 ? DefaultFileSystemCacheSize : FileSystemCacheSize,
                MaxEntries = FileSystemCacheEntries <= 0 ? DefaultFileSystemCacheEntries : FileSystemCacheEntries,
                RetentionPeriod = fileRetention
            };

        if (!TimeSpan.TryParse(MemoryCacheRetention, out var memRetention))
            memRetention = TimeSpan.FromHours(1);

        _tileMemCache = UseMemoryCache
            ? new MemoryCache(_logger)
            {
                MaxBytes = MemoryCacheSize <= 0 ? DefaultMemoryCacheSize : MemoryCacheSize,
                MaxEntries = MemoryCacheEntries <= 0 ? DefaultMemoryCacheEntries : MemoryCacheEntries,
                ParentCache = _tileFileCache,
                RetentionPeriod = memRetention
            }
            : null;

        _cacheIsValid = true;
    }

    public bool UseMemoryCache
    {
        get => (bool)GetValue(UseMemoryCacheProperty);
        set => SetValue(UseMemoryCacheProperty, value);
    }

    public int MemoryCacheSize
    {
        get => (int)GetValue(MemoryCacheSizeProperty);
        set => SetValue(MemoryCacheSizeProperty, value);
    }

    public int MemoryCacheEntries
    {
        get => (int)GetValue(MemoryCacheEntriesProperty);
        set => SetValue(MemoryCacheEntriesProperty, value);
    }

    public string MemoryCacheRetention
    {
        get => (string)GetValue(MemoryCacheRetentionProperty);
        set => SetValue(MemoryCacheRetentionProperty, value);
    }

    public string FileSystemCachePath
    {
        get => (string)GetValue(FileSystemCachePathProperty);
        set => SetValue(FileSystemCachePathProperty, value);
    }

    public int FileSystemCacheSize
    {
        get => (int)GetValue(FileSystemCacheSizeProperty);
        set => SetValue(FileSystemCacheSizeProperty, value);
    }

    public int FileSystemCacheEntries
    {
        get => (int)GetValue(FileSystemCacheEntriesProperty);
        set => SetValue(FileSystemCacheEntriesProperty, value);
    }

    public string FileSystemCacheRetention
    {
        get => (string)GetValue(FileSystemCacheRetentionProperty);
        set => SetValue(FileSystemCacheRetentionProperty, value);
    }
}
