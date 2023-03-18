// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.UI.Xaml.Controls;
using System.Numerics;
using Windows.Foundation;
using J4JSoftware.DeusEx;
using J4JSoftware.J4JMapLibrary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;
using System.Linq;
using J4JSoftware.DependencyInjection;
using J4JSoftware.WindowsAppUtilities;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;
using Serilog;

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

    private enum InsertedTileSide
    {
        Left,
        Right
    }

    private static string GetDefaultFileSystemCachePath()
    {
        var hostConfig = J4JDeusEx.ServiceProvider.GetService<IJ4JHost>();
        if (hostConfig != null)
            return Path.Combine(hostConfig.ApplicationConfigurationFolder, "map-cache");

        J4JDeusEx.OutputFatalMessage( "Could not retrieve instance of IJ4JHost", J4JDeusEx.GetLogger() );
        throw new NullReferenceException("Could not retrieve instance of IJ4JHost");
    }

    private readonly ProjectionFactory _projFactory;
    private readonly ILogger _logger;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly ThrottleDispatcher _throttleFragmentsUpdate = new();

    private IProjection? _projection;
    private ITileCache? _tileMemCache;
    private ITileCache? _tileFileCache;
    private MapFragments? _fragments;
    private bool _suppressLayout;
    private bool _ignoreChange;
    private bool _cacheIsValid;

    public J4JMapControl()
    {
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        _logger = J4JDeusEx.GetLogger() ?? throw new NullReferenceException( "Could not obtain ILogger instance" );
        _logger.ForContext( GetType() );

        _projFactory = J4JDeusEx.ServiceProvider.GetService<ProjectionFactory>()
         ?? throw new NullReferenceException( "Could not create ProjectionFactory" );

        _projFactory.ScanAssemblies();
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

    public string MapStyle
    {
        get => (string)GetValue(MapStyleProperty);
        set => SetValue(MapStyleProperty, value);
    }

    private void UpdateProjection()
    {
        if( !_cacheIsValid )
            UpdateCaching();

        var cache = _tileMemCache ?? _tileFileCache;

        var projResult = _projFactory.CreateProjection( MapName, cache );
        if( !projResult.ProjectionTypeFound )
        {
            J4JDeusEx.OutputFatalMessage($"Could not create projection '{MapName}'", _logger);
            throw new InvalidOperationException( $"Could not create projection '{MapName}'" );
        }

        if( !projResult.Authenticated )
        {
            J4JDeusEx.OutputFatalMessage($"Could not authenticate projection '{MapName}'", _logger);
            throw new InvalidOperationException($"Could not authenticate projection '{MapName}'");
        }

        _projection = projResult.Projection!;

        _fragments = new MapFragments(_projection, _logger);
        _fragments.RetrievalComplete += ( _, _ ) => _dispatcherQueue.TryEnqueue( ()=>OnFragmentsUpdated() );

        InvalidateMeasure();
    }

    public string Center
    {
        get => (string) GetValue( CenterProperty );
        set => SetValue( CenterProperty, value );
    }

    public float CenterLatitude { get; private set; }
    public float CenterLongitude { get; private set; }

    public string MapScale
    {
        get => (string) GetValue( MapScaleProperty );
        set => SetValue( MapScaleProperty, value );
    }

    public int MapNumericScale { get; private set; }

    public string Heading
    {
        get => (string) GetValue( HeadingProperty );
        set => SetValue( HeadingProperty, value );
    }

    public float NumericHeading { get; private set; }

    public bool IsValid
    {
        get => (bool) GetValue( IsValidProperty );
        set => SetValue( IsValidProperty, value );
    }

    private void UpdateFragments()
    {
        if(_projection == null || _fragments == null )
        {
            _logger.Error("Projection not fully initialized");
            return;
        }

        var viewport =  new Viewport(_projection)
        {
            CenterLatitude = CenterLatitude,
            CenterLongitude = CenterLongitude,
            Heading = NumericHeading,
            RequestedHeight = (float)Height,
            RequestedWidth = (float)Width,
            Scale = MapNumericScale
        };

        _fragments.SetViewport( viewport );
        _throttleFragmentsUpdate.Throttle( 250, x=> _fragments.Update() );
        //_fragments.Update();
    }

    private void OnFragmentsUpdated()
    {
        if( _fragments == null )
            return;

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
                AddImageTile(fragment, xTile, yTile, tilePosition);

                tilePosition.Y += fragment?.ImageHeight ?? 0;

                if (fragment?.ImageWidth > maxTileWidth)
                    maxTileWidth = fragment.ImageWidth;
            }

            tilePosition.X += maxTileWidth;

            if (tilePosition.Y > retVal.Height)
                retVal.Height = tilePosition.Y;

            retVal.Width += maxTileWidth;
        }

        // determine the clipping rectangle so we can figure out whether we
        // need to repeat tiles on the left or right
        var clip = new RectangleGeometry()
        {
            Rect = new Rect(new Point(_fragments.CenterPoint.X - Width / 2, _fragments.CenterPoint.Y - Height / 2),
                            new Size(Width, Height))
        };

        // handling tiled projection viewport edge issues...
        if (_projection is ITiledProjection tiledProjection)
        {
            // if the left edge of the clipping rectangle is < 0 we need to insert
            // tiles from the right edge of the map in each row of images
            if (clip.Rect.Left < 0)
                InsertTiles(tiledProjection, InsertedTileSide.Left);
            else
            {
                if (clip.Rect.Right > _fragments.ActualWidth)
                    InsertTiles(tiledProjection, InsertedTileSide.Right);
            }
        }

        Clip = new RectangleGeometry()
        {
            Rect = new Rect(new Point(_fragments.CenterPoint.X - Width / 2, _fragments.CenterPoint.Y - Height / 2),
                            new Size(Width, Height))
        };

        _suppressLayout = false;

        InvalidateMeasure();
    }

    private void AddImageTile( IMapFragment? fragment, int xTile, int yTile, Vector3 tilePosition )
    {
        if (fragment == null)
        {
            _logger.Error("Could not retrieve tile ({0}, {1})", xTile, yTile);
            return;
        }

        var imageBytes = fragment.GetImage();
        if( imageBytes == null )
        {
            _logger.Error("Could not get image data for tile ({0}, {1})", xTile, yTile);
            return;
        }

        var memStream = new MemoryStream( imageBytes );
        
        var bitmapImage = new BitmapImage();
        bitmapImage.SetSource( memStream.AsRandomAccessStream() );

        Children.Add( new Image { Source = bitmapImage, Translation = tilePosition } );
    }

    private void InsertTiles( ITiledProjection projection, InsertedTileSide side )
    {
        // should never happen, but...
        if( _fragments == null )
            return;

        var x = side switch
        {
            InsertedTileSide.Left => -projection.TileHeightWidth,
            InsertedTileSide.Right => projection.GetHeightWidth( _fragments.Scale ),
            _ => throw new InvalidEnumArgumentException( $"Unsupported {typeof( InsertedTileSide )} value '{side}'" )
        };

        var xTile = side switch
        {
            InsertedTileSide.Left => _projection!.GetTileXRange(_fragments.Scale).Maximum,
            InsertedTileSide.Right => _projection!.GetTileXRange(_fragments.Scale).Minimum,
            _ => throw new InvalidEnumArgumentException($"Unsupported {typeof(InsertedTileSide)} value '{side}'")
        };

        var tilePosition = new Vector3(x , 0, 0 );

        for( var yTile = _fragments.YRange.First(); yTile <= _fragments.YRange.Last(); yTile++ )
        {
            var fragment = projection.GetFragment( xTile, yTile, _fragments.Scale );
            AddImageTile( fragment, xTile, yTile, tilePosition );

            tilePosition.Y += fragment?.ImageHeight ?? 0;
        }
    }

    protected override Size MeasureOverride( Size availableSize )
    {
        if( _fragments == null || _suppressLayout )
            return Size.Empty;

        return availableSize;
        //return new Size( _fragments.ImageWidth > availableSize.Width 
        //                     ? availableSize.Width 
        //                     : _fragments.ImageWidth,
        //                 _fragments.ImageHeight > availableSize.Height
        //                     ? availableSize.Height
        //                     : _fragments.ImageHeight );
    }

    protected override Size ArrangeOverride( Size finalSize )
    {
        if( _fragments == null || _suppressLayout )
            return Size.Empty;

        return finalSize;
    }
}
