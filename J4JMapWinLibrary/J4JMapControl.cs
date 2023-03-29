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

    private readonly Grid _imageGrid = new();
    private readonly ProjectionFactory _projFactory;
    private readonly ILogger _logger;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly ThrottleDispatcher _throttleRegionChanges = new();
    private readonly ThrottleDispatcher _throttleUpdates = new();
    private readonly ThrottleDispatcher _throttleMoves = new();

    private IProjection? _projection;
    private ITileCache? _tileMemCache;
    private ITileCache? _tileFileCache;
    private bool _cacheIsValid;
    private PointerPoint? _lastDragPoint;

    public J4JMapControl()
    {
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        _logger = J4JDeusEx.GetLogger() ?? throw new NullReferenceException( "Could not obtain ILogger instance" );
        _logger.ForContext( GetType() );

        _projFactory = J4JDeusEx.ServiceProvider.GetService<ProjectionFactory>()
         ?? throw new NullReferenceException( "Could not create ProjectionFactory" );

        _projFactory.ScanAssemblies();

        Children.Add( _imageGrid );

        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
    }

    private void OnPointerPressed( object sender, PointerRoutedEventArgs e )
    {
        CapturePointer( e.Pointer );
        _lastDragPoint = e.GetCurrentPoint( this );
    }

    private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if( PointerCaptures?.Any( p => p.PointerId == e.Pointer.PointerId ) ?? false )
            OnMapDragged( e.GetIntermediatePoints( this ) );
    }

    private void OnMapDragged( IList<PointerPoint> points )
    {
        // shouldn't be necessary, but...
        if( _lastDragPoint == null || MapRegion == null )
            return;

        foreach( var point in points )
        {
            _throttleMoves.Throttle(5,
                                    _ =>
                                    {
                                        if( _lastDragPoint == null )
                                            return;

                                        var xDelta = point.Position.X - _lastDragPoint.Position.X;
                                        var yDelta = point.Position.Y - _lastDragPoint.Position.Y;

                                        _lastDragPoint = point;

                                        if (Math.Abs(xDelta) == 0 && Math.Abs(yDelta) == 0)
                                            return;

                                        _logger.Warning("Pointer moved {0}, {1}", xDelta, yDelta);

                                        MapRegion.Offset( (float) xDelta, (float) yDelta );
                                        MapRegion.Build();
                                        
                                        InvalidateMeasure();
                                    });
        }
    }

    private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        ReleasePointerCapture( e.Pointer );
        _lastDragPoint = null;
    }

    public int UpdateEventInterval
    {
        get => (int)GetValue(UpdateEventIntervalProperty);
        set => SetValue(UpdateEventIntervalProperty, value);
    }

    private void SetImagePanelTransforms( BuildUpdatedArgument update )
    {
        // define the transform to move and rotate the grid
        var transforms = new TransformGroup();

        transforms.Children.Add( new TranslateTransform() { X = update.Translation.X, Y = update.Translation.Y } );

        transforms.Children.Add(new RotateTransform()
        {
            Angle = update.Rotation,
            CenterX = Width / 2,
            CenterY = Height / 2
        });

        _imageGrid.RenderTransform = transforms;
    }

    private void OnMapRegionLoaded( BuildUpdatedArgument update )
    {
        _imageGrid.Children.Clear();

        // define the required rows and columns
        _imageGrid.RowDefinitions.Clear();
        _imageGrid.ColumnDefinitions.Clear();

        var cellHeightWidth = MapRegion!.ProjectionType switch
        {
            ProjectionType.Static => GridLength.Auto,
            ProjectionType.Tiled => new GridLength( _projection!.TileHeightWidth ),
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {typeof( ProjectionType )} value '{MapRegion.ProjectionType}'" )
        };

        for (var col = 0; col <MapRegion.TilesHigh; col++)
        {
            _imageGrid.ColumnDefinitions.Add( new ColumnDefinition() { Width = cellHeightWidth } );
        }

        for (var row = 0; row < MapRegion.TilesWide; row++)
        {
            _imageGrid.RowDefinitions.Add( new RowDefinition() { Height = cellHeightWidth } );
        }

        SetImagePanelTransforms( update );

        _projection!.LoadRegionAsync( MapRegion );
    }

    private void LoadMapImages()
    {
        foreach (var mapTile in MapRegion!)
        {
            if (!mapTile.InProjection)
                continue;

            var memStream = new MemoryStream(mapTile.ImageData!);

            var bitmapImage = new BitmapImage();
            bitmapImage.SetSource(memStream.AsRandomAccessStream());

            var image = new Image { Source = bitmapImage, Height = mapTile.Height, Width = mapTile.Width, };

            _imageGrid.Children.Add(image);

            // assign the image to the correct grid cell
            Grid.SetColumn(image, mapTile.Column);
            Grid.SetRow(image, mapTile.Row);
        }

        _imageGrid.InvalidateMeasure();
    }

    protected override Size MeasureOverride( Size availableSize )
    {
        if( MapRegion == null || _projection == null )
            return Size.Empty;

        var height = Height < availableSize.Height ? Height : availableSize.Height;
        var width = Width < availableSize.Width ? Width : availableSize.Width;

        MapRegion.Size( (float) height, (float) width );

        return new Size( width, height );
    }

    protected override Size ArrangeOverride( Size finalSize )
    {
        if (MapRegion == null)
            return finalSize;

        if( MapRegion.TilesHigh <= 0 || MapRegion.TilesWide <= 0 )
        {
            _logger.Debug( "MapRegion is empty" );
            _logger.Debug( "ImageGrid has {0} children", _imageGrid.Children.Count );
        }
        else
        {
            _logger.Debug( "MapRegion tile dimensions are {0} x {1}", MapRegion.TilesWide, MapRegion.TilesHigh );
            _logger.Debug("ImageGrid has {0} children", _imageGrid.Children.Count);

            foreach( var childControl in _imageGrid.Children.Cast<FrameworkElement>() )
            {
                var column = Grid.GetColumn( childControl );
                var row = Grid.GetRow( childControl );

                _logger.Debug( "Child grid control ({0}, {1}) is a {2}",
                               column,
                               row,
                               childControl.GetType() );

                if( childControl is Image { Source: BitmapImage bitmapImage } imageControl )
                    _logger.Debug( "Child grid control ({0}, {1}) is a {2} x {3} BitmapImage-based Image control",
                                   column,
                                   row,
                                   bitmapImage.PixelWidth,
                                   bitmapImage.PixelHeight );
            }
        }

        // don't forget to let each child control (e.g., the image panel)
        // participate!
        var rect = new Rect(new Point(), finalSize);

        foreach (var child in Children)
        {
            child.Arrange(rect);
        }

        return finalSize;
    }
}
