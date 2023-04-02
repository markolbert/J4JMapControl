// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using J4JSoftware.DeusEx;
using J4JSoftware.J4JMapLibrary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;
using Windows.Foundation;
using J4JSoftware.DependencyInjection;
using J4JSoftware.J4JMapLibrary.MapRegion;
using J4JSoftware.WindowsAppUtilities;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Serilog;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;
using Path = System.IO.Path;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl : Control
{
    private const int DefaultMemoryCacheSize = 1000000;
    private const int DefaultMemoryCacheEntries = 500;
    private static readonly TimeSpan DefaultMemoryCacheRetention = new ( 1, 0, 0 );
    private const int DefaultFileSystemCacheSize = 10000000;
    private const int DefaultFileSystemCacheEntries = 1000;
    private static readonly TimeSpan DefaultFileSystemCacheRetention = new( 1, 0, 0, 0 );
    private const int DefaultUpdateEventInterval = 250;

    private readonly ILogger _logger;
    private readonly ThrottleDispatcher _throttleScaleChanges = new();

    private Grid? _mapGrid;

    public J4JMapControl()
    {
        DefaultStyleKey = typeof(J4JMapControl);

        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        _logger = J4JDeusEx.GetLogger() ?? throw new NullReferenceException( "Could not obtain ILogger instance" );
        _logger.ForContext( GetType() );

        _projFactory = J4JDeusEx.ServiceProvider.GetService<ProjectionFactory>()
         ?? throw new NullReferenceException( "Could not create ProjectionFactory" );

        _projFactory.ScanAssemblies();

        _movementProcessor = new MovementProcessor();
        _movementProcessor.Moved += MovementProcessorOnMoved;
        _movementProcessor.MovementsEnded += MovementProcessorOnMovementsEnded;

        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;

        this.SizeChanged += OnSizeChanged;
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _mapGrid = FindUIElement<Grid>( "MapGrid" );

        if( _mapGrid != null )
        {
            _mapGrid.PointerWheelChanged += MapGridOnPointerWheelChanged;
        }

        _rotationCanvas = FindUIElement<Canvas>("RotationCanvas");
        _rotationText = FindUIElement<TextBlock>( "RotationText" );
        _rotationLine = FindUIElement<Line>( "RotationLine" );
        _baseLine = FindUIElement<Line>( "BaseLine" );

        _rotationHintsDefined = _rotationCanvas != null
         && _rotationText != null
         && _rotationLine != null
         && _baseLine != null;

        _compassRose = FindUIElement<Image>( "CompassRose" );

        if( _compassRose != null )
        {
            var uri = new System.Uri("ms-appx:///media/rose.png");
            var junk2 = new BitmapImage( uri );

            _compassRose.Source = junk2;
        }

        _scaleSlider = FindUIElement<Slider>("ScaleSlider");

        if( _scaleSlider != null )
            _scaleSlider.ValueChanged += ScaleSliderOnValueChanged;

        SetMapControlMargins( ControlVerticalMargin );
    }

    private void ScaleSliderOnValueChanged( object sender, RangeBaseValueChangedEventArgs e )
    {
        _throttleScaleChanges.Throttle(UpdateEventInterval, _ => MapScale = e.NewValue );
    }

    private T? FindUIElement<T>( string name )
        where T : UIElement
    {
        var retVal = GetTemplateChild( name ) as T;
        if( retVal == null )
            _logger.Error("Couldn't find {0}", name);

        return retVal;
    }

    private void OnSizeChanged( object sender, SizeChangedEventArgs e ) =>
        MapRegion?.Size( (float) e.NewSize.Height, (float) e.NewSize.Width );

    private void SetImagePanelTransforms(RegionBuildResults update)
    {
        if (_mapGrid == null)
            return;

        // define the transform to move and rotate the grid
        var transforms = new TransformGroup();

        transforms.Children.Add(new TranslateTransform() { X = update.Translation.X, Y = update.Translation.Y });

        transforms.Children.Add(new RotateTransform()
        {
            Angle = update.Rotation,
            CenterX = ActualWidth / 2,
            CenterY = ActualHeight / 2
        });

        _mapGrid.RenderTransform = transforms;
    }

    private void LoadMapImages()
    {
        if (_mapGrid == null)
            return;

        DefineColumns();
        DefineRows();

        _mapGrid.Children.Clear();

        foreach( var mapTile in MapRegion! )
        {
            var newImage = new Image { Height = mapTile.Height, Width = mapTile.Width };
            Grid.SetColumn( newImage, mapTile.Column );
            Grid.SetRow( newImage, mapTile.Row );

            if( mapTile is { InProjection: true, ImageData: not null } )
            {
                var memStream = new MemoryStream( mapTile.ImageData );

                var bitmapImage = new BitmapImage();
                bitmapImage.SetSource( memStream.AsRandomAccessStream() );

                newImage.Source = bitmapImage;
            }

            _mapGrid.Children.Add( newImage );
        }

        InvalidateArrange();
    }

    private void DefineColumns()
    {
        var cellWidth = MapRegion!.ProjectionType switch
        {
            ProjectionType.Static => GridLength.Auto,
            ProjectionType.Tiled => new GridLength(MapRegion.TileWidth),
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {typeof(ProjectionType)} value '{MapRegion.ProjectionType}'")
        };

        _mapGrid!.ColumnDefinitions.Clear();

        for (var column = 0; column < MapRegion.TilesWide; column++)
        {
            _mapGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = cellWidth });
        }

    }

    private void DefineRows()
    {
        var cellHeight = MapRegion!.ProjectionType switch
        {
            ProjectionType.Static => GridLength.Auto,
            ProjectionType.Tiled => new GridLength(MapRegion.TileHeight),
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {typeof(ProjectionType)} value '{MapRegion.ProjectionType}'")
        };

        _mapGrid!.RowDefinitions.Clear();

        for (var row = 0; row < MapRegion.TilesHigh; row++)
        {
            _mapGrid.RowDefinitions.Add( new RowDefinition() { Height = cellHeight } );
        }

    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (MapRegion == null || _projection == null)
            return Size.Empty;

        var height = Height < availableSize.Height ? Height : availableSize.Height;
        var width = Width < availableSize.Width ? Width : availableSize.Width;

        return new Size(width, height);
    }
}
