// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using J4JSoftware.DeusEx;
using J4JSoftware.J4JMapLibrary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;
using System.Linq;
using J4JSoftware.DependencyInjection;
using J4JSoftware.J4JMapLibrary.MapRegion;
using J4JSoftware.WindowsAppUtilities;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Serilog;
using Path = System.IO.Path;

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
    private const int DefaultUpdateEventInterval = 250;

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
    private bool _suppressLayout;
    private bool _cacheIsValid;
    private PointerPoint? _dragStart;

    public J4JMapControl()
    {
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        _logger = J4JDeusEx.GetLogger() ?? throw new NullReferenceException( "Could not obtain ILogger instance" );
        _logger.ForContext( GetType() );

        _projFactory = J4JDeusEx.ServiceProvider.GetService<ProjectionFactory>()
         ?? throw new NullReferenceException( "Could not create ProjectionFactory" );

        _projFactory.ScanAssemblies();

        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
    }

    private void OnPointerPressed( object sender, PointerRoutedEventArgs e )
    {
        CapturePointer( e.Pointer );
        _dragStart = e.GetCurrentPoint( this );
    }

    private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if( PointerCaptures?.Any( p => p.PointerId == e.Pointer.PointerId ) ?? false )
            OnMapDragged( e.GetIntermediatePoints( this ) );
    }

    private void OnMapDragged( IList<PointerPoint> points )
    {
        // shouldn't be necessary, but...
        if( _dragStart == null )
            return;

        foreach( var point in points )
        {
            _logger.Warning( "Pointer captured: {0}, {1} (offset from start {2}, {3})",
                             point.Position.X,
                             point.Position.Y,
                             point.Position.X - _dragStart.Position.X,
                             point.Position.Y - _dragStart.Position.Y );
        }
    }

    private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        ReleasePointerCapture( e.Pointer );
        _dragStart = null;
    }

    #region caching

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

    #endregion

    public int UpdateEventInterval
    {
        get => (int)GetValue(UpdateEventIntervalProperty);
        set => SetValue(UpdateEventIntervalProperty, value);
    }

    #region projection

    public string MapProjection
    {
        get => (string) GetValue( MapProjectionProperty );
        set => SetValue( MapProjectionProperty, value );
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

        var projResult = _projFactory.CreateProjection( MapProjection, cache );
        if( !projResult.ProjectionTypeFound )
        {
            J4JDeusEx.OutputFatalMessage($"Could not create projection '{MapProjection}'", _logger);
            throw new InvalidOperationException( $"Could not create projection '{MapProjection}'" );
        }

        if( !projResult.Authenticated )
        {
            J4JDeusEx.OutputFatalMessage($"Could not authenticate projection '{MapProjection}'", _logger);
            throw new InvalidOperationException($"Could not authenticate projection '{MapProjection}'");
        }

        _projection = projResult.Projection!;
        _projection.LoadComplete += ( _, _ ) => _dispatcherQueue.TryEnqueue( OnMapRegionUpdated );

        MapRegion = new MapRegion(_projection, _logger);

        MinScale = _projection.MinScale;
        MaxScale = _projection.MaxScale;

        InvalidateMeasure();
    }

    #endregion

    #region map region

    public MapRegion? MapRegion { get; private set; }

    public string Center
    {
        get => (string) GetValue( CenterProperty );
        set => SetValue( CenterProperty, value );
    }

    public double MapScale
    {
        get => (double) GetValue( MapScaleProperty );
        set => SetValue( MapScaleProperty, value );
    }

    public double MinScale 
    { 
        get => (double ) GetValue( MinScaleProperty );
        private set => SetValue( MinScaleProperty, value );
    }

    public double MaxScale
    {
        get => (double) GetValue( MaxScaleProperty );
        private set => SetValue( MaxScaleProperty, value );
    }

    public double Heading
    {
        get => (double) GetValue( HeadingProperty );
        set => SetValue( HeadingProperty, value );
    }

    private void MapConfigurationChanged()
    {
        if(_projection == null || MapRegion == null )
        {
            _logger.Error("Projection not fully initialized");
            return;
        }

        _throttleFragmentsUpdate.Throttle(UpdateEventInterval < 0 ? DefaultUpdateEventInterval : UpdateEventInterval,
                                           _ => { InvalidateMeasure(); });
    }

    #endregion

    private void OnMapRegionUpdated()
    {
        if( _projection == null || MapRegion == null || _suppressLayout )
            return;

        // make sure we block the measure/arrange cycle
        // we're going to trigger by modifying the Children
        // collection
        //_suppressLayout = true;

        // find or create the grid that contains the map images
        var imagePanel = (Grid?) Children.FirstOrDefault( x => x is Grid );
        if( imagePanel == null )
        {
            imagePanel = new Grid();
            Children.Add( imagePanel );
        }
        else imagePanel.Children.Clear();

        // define the required rows and columns
        imagePanel.RowDefinitions.Clear();
        imagePanel.ColumnDefinitions.Clear();

        var cellHeightWidth = MapRegion.ProjectionType switch
        {
            ProjectionType.Static => GridLength.Auto,
            ProjectionType.Tiled => new GridLength( _projection.TileHeightWidth ),
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {typeof( ProjectionType )} value '{MapRegion.ProjectionType}'" )
        };

        for (var col = 0; col <MapRegion.TilesHigh; col++)
        {
            imagePanel.ColumnDefinitions.Add( new ColumnDefinition() { Width = cellHeightWidth } );
        }

        for (var row = 0; row < MapRegion.TilesWide; row++)
        {
            imagePanel.RowDefinitions.Add( new RowDefinition() { Height = cellHeightWidth } );
        }

        var offset = MapRegion.ViewpointOffset;

        // define the transform to move and rotate the grid
        var transforms = new TransformGroup();
        
        transforms.Children.Add(new TranslateTransform() { X = offset.X, Y = offset.Y });

        transforms.Children.Add( new RotateTransform()
        {
            Angle = MapRegion.Rotation, CenterX = Width / 2, CenterY = Height / 2
        } );

        imagePanel.RenderTransform = transforms;

        // add the individual images to the grid
        foreach( var mapTile in MapRegion )
        {
            if( !mapTile.InProjection )
                continue;

            var relativeCoords = mapTile.GetRelativeTileCoordinates();

            var memStream = new MemoryStream( mapTile.ImageData! );

            var bitmapImage = new BitmapImage();
            bitmapImage.SetSource( memStream.AsRandomAccessStream() );

            var image = new Image { Source = bitmapImage, Height = mapTile.Height, Width = mapTile.Width, };

            imagePanel.Children.Add( image );

            // assign the image to the correct grid cell
            Grid.SetColumn( image, relativeCoords.X );
            Grid.SetRow( image, relativeCoords.Y );
        }
    }

    protected override Size MeasureOverride( Size availableSize )
    {
        if( MapRegion == null || _projection == null )
            return Size.Empty;

        //if( _suppressLayout )
        //{
        //    _suppressLayout = false;
        //    return availableSize;
        //}

        if (!ConverterExtensions.TryParseToLatLong(Center, out var latitude, out var longitude))
        {
            _logger.Error("Could not parse Center ({0})", Center);
            return Size.Empty;
        }

        var height = Height < availableSize.Height ? Height : availableSize.Height;
        var width = Width < availableSize.Width ? Width : availableSize.Width;

        MapRegion
           .Center( latitude, longitude )
           .Scale( (int) MapScale )
           .Heading( (float) Heading )
           .Size( (float) height, (float) width )
           .Build();

        if( MapRegion.ChangedOnBuild )
            _throttleFragmentsUpdate.Throttle(
                UpdateEventInterval < 0 ? DefaultUpdateEventInterval : UpdateEventInterval,
                _ =>
                {
                    _projection.LoadRegionAsync( MapRegion );
                } );

        return new Size( width, height );
    }

    protected override Size ArrangeOverride( Size finalSize )
    {
        if( MapRegion == null || _suppressLayout )
            return finalSize;

        // don't forget to let each child control (e.g., the image panel)
        // participate!
        var rect = new Rect( new Point(), finalSize );

        foreach (var child in Children)
        {
            child.Arrange( rect );
        }

        return finalSize;
    }
}
