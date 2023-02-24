// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
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
using System.Linq;
using System.Runtime.CompilerServices;
using J4JSoftware.DependencyInjection;
using Microsoft.UI.Xaml.Media;

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

    private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    private readonly ProjectionFactory _projFactory;
    private readonly IJ4JLogger _logger;

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

        foreach( var xTile in _fragments.XRange )
        {
            tilePosition.Y = 0;
            var maxTileWidth = 0F;

            foreach( var yTile in _fragments.YRange )
            {
                var fragment = _fragments[ xTile, yTile ];
                AddImageTile( fragment, xTile, yTile, tilePosition );

                tilePosition.Y += fragment?.ActualHeight ?? 0;

                if( fragment?.ActualWidth > maxTileWidth )
                    maxTileWidth = fragment.ActualWidth;
            }

            tilePosition.X += maxTileWidth;

            if( tilePosition.Y > retVal.Height )
                retVal.Height = tilePosition.Y;

            retVal.Width += maxTileWidth;
        }

        // determine the clipping rectangle so we can figure out whether we
        // need to repeat tiles on the left or right
        Clip = new RectangleGeometry()
        {
            Rect = new Rect(new Point(_fragments.CenterPoint.X - Width / 2,
                                      _fragments.CenterPoint.Y - Height / 2),
                            new Size(Width, Height))
        };

        // handling tiled projection viewport edge issues...
        if( _projection is ITiledProjection tiledProjection )
        {
            // if the left edge of the clipping rectangle is < 0 we need to insert
            // tiles from the right edge of the map in each row of images
            if( Clip.Rect.Left < 0 )
                InsertTiles( tiledProjection, InsertedTileSide.Left );
            else
            {
                if( Clip.Rect.Right > _fragments.ActualWidth )
                    InsertTiles( tiledProjection, InsertedTileSide.Right );
            }
        }

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
            InsertedTileSide.Left => -projection.MapServer.TileHeightWidth,
            InsertedTileSide.Right => projection.MapServer.HeightWidth,
            _ => throw new InvalidEnumArgumentException( $"Unsupported {typeof( InsertedTileSide )} value '{side}'" )
        };

        var xTile = side switch
        {
            InsertedTileSide.Left => _fragments.XRange.Last(),
            InsertedTileSide.Right => _fragments.XRange.First(),
            _ => throw new InvalidEnumArgumentException($"Unsupported {typeof(InsertedTileSide)} value '{side}'")
        };

        var tilePosition = new Vector3(x , 0, 0 );

        for ( var yTile = _fragments.YRange.First(); yTile <= _fragments.YRange.Last(); yTile++ )
        {
            var fragment = projection.GetTile(xTile, yTile);
            AddImageTile( fragment, xTile, yTile, tilePosition );

            tilePosition.Y += fragment?.ActualHeight ?? 0;
        }
    }

    protected override Size MeasureOverride( Size availableSize )
    {
        if( _fragments == null || _suppressLayout )
            return Size.Empty;

        return availableSize;
        //return new Size( _fragments.ActualWidth > availableSize.Width 
        //                     ? availableSize.Width 
        //                     : _fragments.ActualWidth,
        //                 _fragments.ActualHeight > availableSize.Height
        //                     ? availableSize.Height
        //                     : _fragments.ActualHeight );
    }

    protected override Size ArrangeOverride( Size finalSize )
    {
        if( _fragments == null || _suppressLayout )
            return Size.Empty;

        return finalSize;
    }
}
