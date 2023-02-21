// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using Microsoft.UI.Xaml.Controls;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using J4JSoftware.DeusEx;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;
using J4JSoftware.DependencyInjection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl : Panel
{
    private const int DefaultMemoryCacheSize = 1000000;
    private const int DefaultMemoryCacheEntries = 500;
    private static readonly TimeSpan DefaultMemoryCacheRetention = new ( 1, 0, 0 );
    private const int DefaultFileSystemCacheSize = 10000000;
    private const int DefaultFileSystemCacheEntries = 1000;
    private static readonly TimeSpan DefaultFileSystemCacheRetention = new( 1, 0, 0, 0 );

    private static string GetDefaultFileSystemCachePath()
    {
        var hostConfig = J4JDeusEx.ServiceProvider.GetService<IJ4JHost>();
        if (hostConfig != null)
            return Path.Combine(hostConfig.ApplicationConfigurationFolder, "map-cache");

        J4JDeusEx.OutputFatalMessage( "Could not retrieve instance of IJ4JHost", J4JDeusEx.GetLogger() );
        throw new NullReferenceException("Could not retrieve instance of IJ4JHost");
    }

    private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    private readonly IJ4JLogger _logger;
    private readonly IProjectionCredentials _projCredentials;

    private IProjection? _projection;
    private ITileCache? _tileMemCache;
    private ITileCache? _tileFileCache;
    private MapFragments? _fragments;
    private bool _suppressLayout;
    private bool _cacheIsValid;

    public J4JMapControl()
    {
        _logger = J4JDeusEx.GetLogger() ?? throw new NullReferenceException( "Could not obtain IJ4JLogger instance" );
        _logger.SetLoggedType( GetType() );

        var tempCredentials = J4JDeusEx.ServiceProvider.GetService<IProjectionCredentials>();
        if( tempCredentials == null )
        {
            _logger.Fatal( "Could not obtain map projection credentials" );
            throw new NullReferenceException( "Could not obtain map projection credentials" );
        }

        _projCredentials = tempCredentials;
    }

    public bool UseMemoryCache
    {
        get => (bool) GetValue( UseMemoryCacheProperty );
        set => SetValue( UseMemoryCacheProperty, value );
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
        get => (string) GetValue( MemoryCacheRetentionProperty );
        set => SetValue( MemoryCacheRetentionProperty, value );
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

        if (!TimeSpan.TryParse(MemoryCacheRetention, out var memRetention))
            memRetention = TimeSpan.FromHours(1);

        _tileMemCache = UseMemoryCache
            ? new MemoryCache( _logger )
            {
                MaxBytes = MemoryCacheSize <= 0 ? DefaultMemoryCacheSize : MemoryCacheSize, 
                MaxEntries = MemoryCacheEntries <= 0 ?DefaultMemoryCacheEntries : MemoryCacheEntries, 
                ParentCache = _tileFileCache,
                RetentionPeriod = memRetention
            }
            : null;

        _cacheIsValid = true;
    }

    public string MapName
    {
        get => (string) GetValue( MapNameProperty );
        set => SetValue( MapNameProperty, value );
    }

    private void UpdateProjection()
    {
        if( !_cacheIsValid )
            UpdateCaching();

        var cache = _tileMemCache ?? _tileFileCache;

        var tempProjection = MapName switch
        {
            "BingMaps" => (IProjection) new BingMapsProjection( _projCredentials, _logger, cache ),
            "OpenStreetMaps" => new OpenStreetMapsProjection( _projCredentials, _logger, cache ),
            "OpenTopoMaps" => new OpenTopoMapsProjection( _projCredentials, _logger, cache ),
            "GoogleMaps" => new GoogleMapsProjection( _projCredentials, _logger ),
            _ => null
        };

        if (tempProjection == null)
        {
            _logger.Fatal<string>("Unsupported map projection '{0}'", MapName);
            throw new NullReferenceException($"Unsupported map projection '{MapName}'");
        }

        if (!tempProjection.Authenticate(null))
            _logger.Error<string>("Authentication of {0} failed", MapName);

        _projection = tempProjection;
        _fragments = new MapFragments(_projection, _logger);
        InvalidateMeasure();
    }

    public string Center
    {
        get => (string) GetValue( CenterProperty );
        set => SetValue( CenterProperty, value );
    }

    public string MapScale
    {
        get => (string) GetValue( MapScaleProperty );
        set => SetValue( MapScaleProperty, value );
    }

    public string Heading
    {
        get => (string) GetValue( HeadingProperty );
        set => SetValue( HeadingProperty, value );
    }

    private void UpdateChildControls()
    {
        if( _fragments == null )
            return;

        if( !ConverterExtensions.TryParseToLatLong( Center, out var latitude, out var longitude ) )
            return;

        _fragments.SetCenter( latitude, longitude );

        if( !int.TryParse(MapScale, out var mapScale))
            return;

        _fragments.Scale = mapScale;

        if( !float.TryParse( Heading, out var heading ) )
            return;

        _fragments.Heading = heading;

        _fragments.SetRequestedHeightWidth(Height, Width);
        
        Task.Run(async () => await _fragments.UpdateAsync()).Wait();

        _suppressLayout = true;

        Children.Clear();

        var tilePosition = new Vector3(0, 0, 0);
        var retVal = new Size();

        foreach (var xTile in _fragments.XRange)
        {
            tilePosition.Y = 0;
            var maxTileWidth = 0F;

            foreach (var yTile in _fragments.YRange)
            {
                var fragment = _fragments[xTile, yTile];

                if (fragment == null)
                {
                    _logger.Error("Could not retrieve tile ({0}, {1})", xTile, yTile);
                    continue;
                }

                var imageBytes = fragment.GetImage();
                if (imageBytes == null)
                    continue;

                var memStream = new MemoryStream(imageBytes);
                var bitmapImage = new BitmapImage();
                bitmapImage.SetSource(memStream.AsRandomAccessStream());
                var image = new Image { Source = bitmapImage, Translation = tilePosition };

                Children.Add(image);

                tilePosition.Y += fragment.ActualHeight;

                if (fragment.ActualWidth > maxTileWidth)
                    maxTileWidth = fragment.ActualWidth;
            }

            tilePosition.X += maxTileWidth;

            if (tilePosition.Y > retVal.Height)
                retVal.Height = tilePosition.Y;

            retVal.Width += maxTileWidth;
        }

        _suppressLayout = false;

        InvalidateMeasure();
    }

    protected override Size MeasureOverride( Size availableSize )
    {
        if( _fragments == null || _suppressLayout )
            return Size.Empty;

        return new Size( _fragments.ActualWidth > availableSize.Width 
                             ? availableSize.Width 
                             : _fragments.ActualWidth,
                         _fragments.ActualHeight > availableSize.Height
                             ? availableSize.Height
                             : _fragments.ActualHeight );
    }

    protected override Size ArrangeOverride( Size finalSize )
    {
        if( _fragments == null || _suppressLayout )
            return Size.Empty;

        return finalSize;
    }
}
