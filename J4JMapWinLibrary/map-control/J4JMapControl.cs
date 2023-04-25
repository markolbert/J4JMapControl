#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// J4JMapControl.cs
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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.WindowsUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using System.Xml.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl : Control
{
    private const int DefaultMemoryCacheSize = 1000000;
    private const int DefaultMemoryCacheEntries = 500;
    private const int DefaultFileSystemCacheSize = 10000000;
    private const int DefaultFileSystemCacheEntries = 1000;
    internal const int DefaultUpdateEventInterval = 250;
    private static readonly TimeSpan DefaultMemoryCacheRetention = new( 1, 0, 0 );
    private static readonly TimeSpan DefaultFileSystemCacheRetention = new( 1, 0, 0, 0 );

    private readonly ThrottleDispatcher _throttleScaleChanges = new();

    private Grid? _mapGrid;
    private ILoggerFactory? _loggerFactory;
    private ILogger? _logger;
    private int _updateInterval = DefaultUpdateEventInterval;

    public J4JMapControl()
    {
        DefaultStyleKey = typeof( J4JMapControl );

        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        _movementProcessor = new MovementProcessor();
        _movementProcessor.Moved += MovementProcessorOnMoved;
        _movementProcessor.MovementsEnded += MovementProcessorOnMovementsEnded;

        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;

        SizeChanged += OnSizeChanged;

        _routeSourceValidator = new DataSourceValidator<J4JMapControl>(this);
        _routeSourceValidator.AddRule("RoutesLocationProperty",
                                      x => x.RoutesLocationProperty,
                                      typeof(string));

        _pointsOfInterest = new PointsOfInterestPositions( this, x => x.PointsOfInterestTemplate );
        _pointsOfInterest.SourceUpdated += PointsOfInterestSourceUpdated;
    }

    public ILoggerFactory? LoggerFactory
    {
        get => _loggerFactory;

        set
        {
            _loggerFactory = value;
            _logger = _loggerFactory?.CreateLogger<J4JMapControl>();
        }
    }

    public int UpdateEventInterval
    {
        get => _updateInterval;
        set => _updateInterval = value < 0 ? DefaultUpdateEventInterval : value;
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _mapGrid = FindUIElement<Grid>( "MapGrid", x => x.PointerWheelChanged += MapGridOnPointerWheelChanged );
        _annotationsCanvas = FindUIElement<Canvas>( "AnnotationsCanvas" );
        _poiCanvas = FindUIElement<Canvas>( "PoICanvas" );
        _routesCanvas = FindUIElement<Canvas>("RoutesCanvas");

        _rotationCanvas = FindUIElement<Canvas>( "RotationCanvas" );
        _rotationPanel = FindUIElement<StackPanel>( "RotationPanel" );
        _rotationText = FindUIElement<TextBlock>( "RotationText" );
        _headingText = FindUIElement<TextBlock>( "HeadingText" );
        _rotationLine = FindUIElement<Line>( "RotationLine" );
        _baseLine = FindUIElement<Line>( "BaseLine" );

        _rotationHintsDefined = _rotationCanvas != null
         && _rotationPanel != null
         && _rotationText != null
         && _headingText != null
         && _rotationLine != null
         && _baseLine != null;

        _compassRose = FindUIElement<Image>( "CompassRose" );
        _scaleSlider = FindUIElement<Slider>( "ScaleSlider", x => x.ValueChanged += ScaleSliderOnValueChanged );

        SetMapControlMargins( ControlVerticalMargin );
    }

    private void ScaleSliderOnValueChanged( object sender, RangeBaseValueChangedEventArgs e )
    {
        _throttleScaleChanges.Throttle( UpdateEventInterval, _ => MapScale = e.NewValue );
    }

    private T? FindUIElement<T>( string name, Action<T>? postProcessor = null )
        where T : UIElement
    {
        var retVal = GetTemplateChild( name ) as T;
        if( retVal == null )
            _logger?.LogError( "Couldn't find {name}", name );
        else postProcessor?.Invoke( retVal );

        return retVal;
    }

    private void OnSizeChanged( object sender, SizeChangedEventArgs e ) =>
        MapRegion?.Size( (float) e.NewSize.Height, (float) e.NewSize.Width );

    private void SetImagePanelTransforms()
    {
        if( _mapGrid == null || MapRegion == null )
            return;

        // define the transform to move and rotate the grid
        var transforms = new TransformGroup();

        transforms.Children.Add( new TranslateTransform { X = MapRegion.ViewpointOffset.X, Y = MapRegion.ViewpointOffset.Y } );

        transforms.Children.Add( new RotateTransform
        {
            Angle = MapRegion.Rotation, CenterX = ActualWidth / 2, CenterY = ActualHeight / 2
        } );

        _mapGrid.RenderTransform = transforms;
    }

    private void LoadMapImages()
    {
        if( _mapGrid == null )
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

    private void IncludeAnnotations()
    {
        if( _annotationsCanvas == null || MapRegion == null )
            return;

        _annotationsCanvas.Children.Clear();

        foreach (var element in Annotations)
        {
            if (!Location.InRegion(element, MapRegion))
                continue;

            _annotationsCanvas.Children.Add(element);
        }
    }

    private void IncludePointsOfInterest()
    {
        if( _poiCanvas == null || MapRegion == null )
            return;

        _poiCanvas.Children.Clear();

        foreach( var item in _pointsOfInterest.PlacedItems )
        {
            if( item is not PlacedPointOfInterest poiItem
            || !Location.InRegion( item, MapRegion )
            || poiItem.VisualElement == null )
                continue;

            PositionVisualElement( poiItem.VisualElement, poiItem.Latitude, poiItem.Longitude );

            _poiCanvas.Children.Add( poiItem.VisualElement );
        }
    }

    private void PositionVisualElement( FrameworkElement element, float latitude, float longitude )
    {
        if( MapRegion == null )
            return;

        var mapPoint = new MapPoint(MapRegion);
        mapPoint.SetLatLong(latitude, longitude);

        var (offsetX, offsetY) = MapRegion.UpperLeft.GetUpperLeftCartesian();

        var offset = MapRegion.ViewpointOffset;
        offset.X += mapPoint.X - offsetX;
        offset.Y += mapPoint.Y - offsetY;

        if (MapRegion.Rotation % 360 != 0)
        {
            var centerPoint = new Vector3(MapRegion.RequestedWidth / 2, MapRegion.RequestedHeight / 2, 0);

            var transform =
                Matrix4x4.CreateRotationZ(MapRegion.Rotation * MapConstants.RadiansPerDegree, centerPoint);
            offset = Vector3.Transform(offset, transform);
        }

        // get the relative horizontal/vertical offsets for the UIElement
        offset.X += (float)(HorizontalAlignment switch
        {
            HorizontalAlignment.Left => -element.ActualWidth,
            HorizontalAlignment.Right => 0,
            _ => -element.ActualWidth / 2
        });

        offset.Y += (float)(VerticalAlignment switch
        {
            VerticalAlignment.Top => 0,
            VerticalAlignment.Bottom => -element.ActualHeight,
            _ => -element.ActualHeight / 2
        });

        // get the specific/custom offset
        Location.TryParseOffset(this, out var customOffset);

        offset.X += (float)customOffset.X;
        offset.Y += (float)customOffset.Y;

        Canvas.SetLeft(element, offset.X);
        Canvas.SetTop(element, offset.Y);
    }

    private void IncludeRoutes()
    {
        if (_routesCanvas== null || MapRegion == null || _routePoints == null)
            return;

    }

    private void DefineColumns()
    {
        var cellWidth = MapRegion!.ProjectionType switch
        {
            ProjectionType.Static => GridLength.Auto,
            ProjectionType.Tiled => new GridLength( MapRegion.TileWidth ),
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {typeof( ProjectionType )} value '{MapRegion.ProjectionType}'" )
        };

        _mapGrid!.ColumnDefinitions.Clear();

        for( var column = 0; column < MapRegion.TilesWide; column++ )
        {
            _mapGrid.ColumnDefinitions.Add( new ColumnDefinition { Width = cellWidth } );
        }
    }

    private void DefineRows()
    {
        var cellHeight = MapRegion!.ProjectionType switch
        {
            ProjectionType.Static => GridLength.Auto,
            ProjectionType.Tiled => new GridLength( MapRegion.TileHeight ),
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {typeof( ProjectionType )} value '{MapRegion.ProjectionType}'" )
        };

        _mapGrid!.RowDefinitions.Clear();

        for( var row = 0; row < MapRegion.TilesHigh; row++ )
        {
            _mapGrid.RowDefinitions.Add( new RowDefinition { Height = cellHeight } );
        }
    }

    protected override Size MeasureOverride( Size availableSize )
    {
        if( MapRegion == null || _projection == null )
            return availableSize;

        var height = Height < availableSize.Height ? Height : availableSize.Height;
        var width = Width < availableSize.Width ? Width : availableSize.Width;

        return new Size( width, height );
    }

    protected override Size ArrangeOverride( Size finalSize )
    {
        base.ArrangeOverride( finalSize );

        ArrangeAnnotations();

        return finalSize;
    }

    private void ArrangeAnnotations()
    {
        if( _annotationsCanvas == null || MapRegion == null )
            return;

        foreach( var element in _annotationsCanvas.Children
                                                  .Where(x=>x is FrameworkElement  )
                                                  .Cast<FrameworkElement>() )
        {
            Location.TryParseLatLong( element, out var latitude, out var longitude );
            PositionVisualElement( element, latitude, longitude );
        }
    }
}
