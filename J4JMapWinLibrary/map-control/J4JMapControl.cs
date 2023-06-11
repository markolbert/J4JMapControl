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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.VisualUtilities;
using J4JSoftware.WindowsUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl : Control
{
    private enum ProjectionSide
    {
        North,
        South,
        West,
        East
    }

    private const int DefaultMemoryCacheSize = 1000000;
    private const int DefaultMemoryCacheEntries = 500;
    private const int DefaultFileSystemCacheSize = 10000000;
    private const int DefaultFileSystemCacheEntries = 1000;
    internal const int DefaultUpdateEventInterval = 250;
    private const int DefaultControlHeight = 300;
    private static readonly TimeSpan DefaultMemoryCacheRetention = new( 1, 0, 0 );
    private static readonly TimeSpan DefaultFileSystemCacheRetention = new( 1, 0, 0, 0 );

    private readonly ILogger? _logger;
    private readonly ThrottleDispatcher _throttleScaleChanges = new();
    private readonly ThrottleDispatcher _throttleSizeChanges = new();

    private Grid? _mapGrid;
    private int _updateInterval = DefaultUpdateEventInterval;

    public J4JMapControl()
    {
        if( MapControlViewModelLocator.Instance == null )
            throw new NullReferenceException( $"{typeof( MapControlViewModelLocator )} was not initialized" );

        _logger = MapControlViewModelLocator.Instance.LoggerFactory?.CreateLogger<J4JMapControl>();

        DefaultStyleKey = typeof( J4JMapControl );

        MapProjections = MapControlViewModelLocator.Instance
            .ProjectionFactory
            .ProjectionNames
            .ToList();

        MapProjection = MapProjections.FirstOrDefault();

        _movementProcessor = new MovementProcessor();
        _movementProcessor.Moved += MovementProcessorOnMoved;
        _movementProcessor.MovementsEnded += MovementProcessorOnMovementsEnded;

        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;

        DoubleTapped += OnDoubleTapped;

        SizeChanged += OnSizeChanged;

        _pointsOfInterest = new PointsOfInterestPositions( this, x => x.PointsOfInterestTemplate );
        _pointsOfInterest.SourceUpdated += PointsOfInterestSourceUpdated;

        Loaded += OnLoaded;
    }

    private async void OnLoaded( object sender, RoutedEventArgs e )
    {
        await InitializeProjectionAsync();
    }

    public int UpdateEventInterval
    {
        get => _updateInterval;
        set => _updateInterval = value < 0 ? DefaultUpdateEventInterval : value;
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _mapGrid = FindUiElement<Grid>( "MapGrid", x => x.PointerWheelChanged += MapGridOnPointerWheelChanged );
        _controlGrid = FindUiElement<Grid>( "ControlGrid" );
        _annotationsCanvas = FindUiElement<Canvas>( "AnnotationsCanvas" );
        _poiCanvas = FindUiElement<Canvas>( "PoICanvas" );
        _routesCanvas = FindUiElement<Canvas>( "RoutesCanvas" );

        _rotationCanvas = FindUiElement<Canvas>( "RotationCanvas" );
        _rotationPanel = FindUiElement<StackPanel>( "RotationPanel" );
        _rotationText = FindUiElement<TextBlock>( "RotationText" );
        _headingText = FindUiElement<TextBlock>( "HeadingText" );
        _rotationLine = FindUiElement<Line>( "RotationLine" );
        _baseLine = FindUiElement<Line>( "BaseLine" );

        _rotationHintsDefined = _rotationCanvas != null
         && _rotationPanel != null
         && _rotationText != null
         && _headingText != null
         && _rotationLine != null
         && _baseLine != null;

        _compassRose = FindUiElement<Image>( "CompassRose" );
        _scaleSlider = FindUiElement<Slider>( "ScaleSlider", x => x.ValueChanged += ScaleSliderOnValueChanged );

        SetMapControlMargins( ControlVerticalMargin );
    }

    private void ScaleSliderOnValueChanged( object sender, RangeBaseValueChangedEventArgs e )
    {
        _throttleScaleChanges.Throttle( UpdateEventInterval, _ => MapScale = e.NewValue );
    }

    private T? FindUiElement<T>( string name, Action<T>? postProcessor = null )
        where T : UIElement
    {
        var retVal = GetTemplateChild( name ) as T;
        if( retVal == null )
            _logger?.LogError( "Couldn't find {name}", name );
        else postProcessor?.Invoke( retVal );

        return retVal;
    }

    private void OnSizeChanged( object sender, SizeChangedEventArgs e )
    {
        if( e.NewSize.Width <= 0 || e.NewSize.Height <= 0 )
            return;

        _throttleSizeChanges.Throttle( UpdateEventInterval, _ =>
        {
            var adjSize = AdjustSizeForViewToRegionScaling(e.NewSize);
            MapRegion?.Size( adjSize.Height, adjSize.Width );
        } );

        _throttleSliderSizeChange.Throttle( UpdateEventInterval, _ => SetControlGridSizes( e.NewSize ) );
    }

    private (float Height, float Width) AdjustSizeForViewToRegionScaling( Vector2 size ) =>
        AdjustSizeForViewToRegionScaling( new Size( size.X, size.Y ) );

    private (float Height, float Width) AdjustSizeForViewToRegionScaling( Size newSize )
    {
        if( MapRegion == null )
            return ( (float) newSize.Height, (float) newSize.Width );

        var scalingFactors = GetRegionToViewScalingFactors( newSize );

        return scalingFactors != null
            ? ( scalingFactors.Value * (float) newSize.Height,
                scalingFactors.Value * (float) newSize.Width )
            : ( (float) newSize.Height, (float) newSize.Width );
    }

    private float? GetRegionToViewScalingFactors( Vector2 size ) =>
        GetRegionToViewScalingFactors( new Size( size.X, size.Y ) );

    private float? GetRegionToViewScalingFactors( Size size )
    {
        if( MapRegion == null || StretchStyle == MapStretchStyle.None
           || size.Height == 0 || size.Width == 0 )
            return null;

        var center = new Vector3( MapRegion.Center.X, MapRegion.Center.Y, 0 );

        var projWidthHeight = MapRegion.Projection.GetHeightWidth((int)MapScale);

        var viewRect = new Rectangle2D( (float) size.Height,
                                        (float) size.Width,
                                        MapRegion.Rotation,
                                        center );

        var northZoom = GetCornerScalingFactor( viewRect, projWidthHeight, ProjectionSide.North ) ?? float.PositiveInfinity;
        var southZoom = GetCornerScalingFactor( viewRect, projWidthHeight, ProjectionSide.South ) ?? float.NegativeInfinity;
        var westZoom = GetCornerScalingFactor( viewRect, projWidthHeight, ProjectionSide.West ) ?? float.PositiveInfinity;
        var eastZoom = GetCornerScalingFactor( viewRect, projWidthHeight, ProjectionSide.East ) ?? float.NegativeInfinity;

        var zooms = CreateFiniteZooms(northZoom,southZoom, westZoom, eastZoom );

        return StretchStyle switch
        {
            MapStretchStyle.FitHeight => northZoom < southZoom ? northZoom : southZoom,
            MapStretchStyle.FitWidth => westZoom < eastZoom ? westZoom : eastZoom,
            MapStretchStyle.PreserveAspectRatio => zooms.Min(),
            MapStretchStyle.None => (float?) null,
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {typeof( MapStretchStyle )} value '{StretchStyle}'" )
        };

        //if( projWidthHeight >= size.Height && projWidthHeight >= size.Width )
        //    return null;

        //var heightScale = projWidthHeight / (float) size.Height;
        //var widthScale = projWidthHeight / (float) size.Width;

        //return StretchStyle switch
        //{
        //    MapStretchStyle.FitHeight => ( 1f, heightScale  ),
        //    MapStretchStyle.FitWidth => ( widthScale, 1f ),
        //    MapStretchStyle.PreserveAspectRatio => heightScale < widthScale
        //        ? ( heightScale , heightScale  )
        //        : ( widthScale ,widthScale  ),
        //    _ => null
        //};
    }

    public List<float> CreateFiniteZooms( params float[] zooms ) => zooms.Where( float.IsFinite ).ToList();

    private float? GetCornerScalingFactor( Rectangle2D viewRect, float projHeightWidth, ProjectionSide side )
    {
        if( viewRect.Height <= 0 || viewRect.Width <= 0 )
            return null;

        Vector3? minMaxCorner = null;
        var minMaxValue = side is ProjectionSide.North or ProjectionSide.West ? float.PositiveInfinity : float.NegativeInfinity;

        foreach( var corner in viewRect )
        {
            var curValue = side switch
            {
                ProjectionSide.North => corner.Y,
                ProjectionSide.West => corner.X,
                ProjectionSide.South => corner.Y,
                ProjectionSide.East => corner.X,
                _ => throw new InvalidEnumArgumentException( $"Invalid {typeof( ProjectionSide )} value '{side}'" )
            };

            // ReSharper disable once InvertIf
            if( ( side is ProjectionSide.North or ProjectionSide.West && curValue < minMaxValue )
            || ( side is ProjectionSide.South or ProjectionSide.East && curValue > minMaxValue ) )
            {
                minMaxValue = curValue;
                minMaxCorner = corner;
            }
        }

        // should never happen, but
        if( minMaxCorner == null )
        {
            _logger?.LogWarning( "Could not find minMaxCorner" );
            return null;
        }

        var deltaCornerX = minMaxCorner.Value.X - viewRect.Center.X;
        var deltaCornerY = minMaxCorner.Value.Y - viewRect.Center.Y;
        var slope = deltaCornerY / deltaCornerX;

        var centerToCorner = (float) Math.Sqrt( deltaCornerX * deltaCornerX + deltaCornerY * deltaCornerY );

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch( side )
        {
            case ProjectionSide.North:
            case ProjectionSide.South:
                var yValue = side == ProjectionSide.North ? -viewRect.Center.Y : projHeightWidth - viewRect.Center.Y;
                var xIntersect = viewRect.Center.X + yValue / slope;

                return Distance( viewRect.Center, new Vector3( xIntersect, yValue, 0 ) ) / centerToCorner;

            case ProjectionSide.West:
            case ProjectionSide.East:
                var xValue = side == ProjectionSide.West ? -viewRect.Center.X : projHeightWidth - viewRect.Center.X;
                var yIntersect = viewRect.Center.Y + slope * xValue;

                return Distance( viewRect.Center, new Vector3( xValue, yIntersect, 0 ) ) / centerToCorner;
        }

        throw new InvalidEnumArgumentException( $"Invalid {typeof( ProjectionSide )} value '{side}'" );
    }

    private float Distance( Vector3 pt1, Vector3 pt2 )
    {
        var xDelta = pt2.X - pt1.X;
        var yDelta = pt2.Y - pt1.Y;
        var zDelta = pt2.Z - pt1.Z;

        return (float) Math.Sqrt( xDelta * xDelta + yDelta * yDelta + zDelta * zDelta );
    }

    private Point ViewPointToRegionPoint( Point point )
    {
        if( MapRegion == null || StretchStyle == MapStretchStyle.None )
            return point;

        var scalingFactors = GetRegionToViewScalingFactors( ActualSize );

        return scalingFactors == null
            ? point
            : new Point( point.X * scalingFactors.Value, point.Y * scalingFactors.Value );
    }

    private Point RegionPointToViewPoint(Point point)
    {
        if (MapRegion == null || StretchStyle == MapStretchStyle.None)
            return point;

        var scalingFactors = GetRegionToViewScalingFactors(ActualSize);

        return scalingFactors == null
            ? point
            : new Point(point.X / scalingFactors.Value, point.Y / scalingFactors.Value);
    }

    private void SetImagePanelTransforms()
    {
        if( _mapGrid == null || MapRegion == null )
            return;

        // define the transform to move and rotate the grid
        var transforms = new TransformGroup();

        var scalingFactors = GetRegionToViewScalingFactors( new Size( ActualWidth, ActualHeight ) );

        if( scalingFactors != null )
        {
            transforms.Children.Add(new ScaleTransform
            {
                ScaleX = 1 / scalingFactors.Value,
                ScaleY = 1 / scalingFactors.Value
            });
        }

        transforms.Children.Add( new TranslateTransform
        {
            X = scalingFactors == null
                ? MapRegion.ViewpointOffset.X
                : MapRegion.ViewpointOffset.X / scalingFactors.Value,
            Y = scalingFactors == null
                ? MapRegion.ViewpointOffset.Y
                : MapRegion.ViewpointOffset.Y / scalingFactors.Value
        } );

        transforms.Children.Add( new RotateTransform
        {
            Angle = MapRegion.Rotation,
            CenterX = ActualWidth / 2,
            CenterY = ActualHeight / 2
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

        for( var row = 0; row < MapRegion!.TilesHigh; row++ )
        {
            for( var column = 0; column < MapRegion.TilesWide; column++ )
            {
                var mapTile = MapRegion[ column, row ];
                if( mapTile == null )
                    continue;

                var newImage = new Image { Height = mapTile.Height, Width = mapTile.Width };
                Grid.SetColumn( newImage, column );
                Grid.SetRow( newImage, row );

                if( mapTile is { ImageData: not null } )
                {
                    var memStream = new MemoryStream( mapTile.ImageData );

                    var bitmapImage = new BitmapImage();
                    bitmapImage.SetSource( memStream.AsRandomAccessStream() );

                    newImage.Source = bitmapImage;
                }

                _mapGrid.Children.Add( newImage );
            }
        }

        InvalidateArrange();
    }

    private void IncludeAnnotations()
    {
        if( _annotationsCanvas == null || MapRegion == null )
            return;

        _annotationsCanvas.Children.Clear();

        foreach( var element in Annotations )
        {
            if( !Location.InRegion( element, MapRegion ) )
                continue;

            _annotationsCanvas.Children.Add( element );
        }
    }

    private void IncludePointsOfInterest()
    {
        if( _poiCanvas == null || MapRegion == null )
            return;

        _poiCanvas.Children.Clear();

        foreach( var item in _pointsOfInterest.PlacedItems )
        {
            if( item is not PlacedTemplatedElement poiItem
            || !Location.InRegion( item, MapRegion )
            || poiItem.VisualElement == null )
                continue;

            var elementOffset = MapRegion.GetDisplayPosition( poiItem.Latitude, poiItem.Longitude );
            poiItem.VisualElement.PositionRelativeToPoint( elementOffset );

            _poiCanvas.Children.Add( poiItem.VisualElement );
        }
    }

    private void IncludeRoutes()
    {
        if( _routesCanvas == null || MapRegion == null )
            return;

        _routesCanvas.Children.Clear();

        foreach( var route in MapRoutes )
        {
            if( route.RoutePositions == null )
                continue;

            for( var ptNum = 0; ptNum < route.RoutePositions.PlacedItems.Count; ptNum++ )
            {
                var curPt = route.RoutePositions.PlacedItems[ ptNum ];
                if( !Location.InRegion( curPt, MapRegion ) )
                    continue;

                var nextPt = ptNum >= route.RoutePositions.PlacedItems.Count - 1
                    ? null
                    : route.RoutePositions.PlacedItems[ ptNum + 1 ];

                if( curPt is not IPlacedElement curPlaced )
                    continue;

                var curPtPosition = MapRegion.GetDisplayPosition( curPlaced.Latitude, curPlaced.Longitude );

                if( curPlaced.VisualElement != null )
                    curPtPosition = curPlaced.VisualElement.PositionRelativeToPoint( curPtPosition );

                if( nextPt is not IPlacedElement nextPlaced || nextPlaced.VisualElement == null )
                {
                    if( route.ShowPoints )
                        _routesCanvas.Children.Add( curPlaced.VisualElement );

                    continue;
                }

                var nextPtPosition = MapRegion.GetDisplayPosition( nextPlaced.Latitude, nextPlaced.Longitude );

                if( nextPlaced.VisualElement != null )
                    nextPtPosition = nextPlaced.VisualElement.PositionRelativeToPoint( nextPtPosition );

                var curElementSize = curPlaced.VisualElement == null
                    ? new Vector3()
                    : new Vector3( (float) curPlaced.VisualElement.Width, (float) curPlaced.VisualElement.Height, 0 );

                var nextElementSize = nextPlaced.VisualElement == null
                    ? new Vector3()
                    : new Vector3( (float) nextPlaced.VisualElement.Width, (float) nextPlaced.VisualElement.Height, 0 );

                var line = new Line
                {
                    X1 = curPtPosition.X + curElementSize.X / 2,
                    Y1 = curPtPosition.Y + curElementSize.Y / 2,
                    X2 = nextPtPosition.X + nextElementSize.X / 2,
                    Y2 = nextPtPosition.Y + nextElementSize.Y / 2,
                    Stroke = new SolidColorBrush( route.StrokeColor ),
                    StrokeThickness = route.StrokeWidth,
                    Opacity = route.StrokeOpacity
                };

                _routesCanvas.Children.Add( line );

                if( route.ShowPoints )
                    _routesCanvas.Children.Add( curPlaced.VisualElement );
            }
        }
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
        availableSize = SizeIsValid( availableSize ) ? availableSize : new Size( 100, 100 );

        if( MapRegion == null || _projection == null )
            return availableSize;

        var height = Height < availableSize.Height ? Height : availableSize.Height;
        var width = Width < availableSize.Width ? Width : availableSize.Width;

        return new Size( width, height );
    }

    private bool SizeIsValid( Size toCheck ) =>
        toCheck.Width != 0
     && toCheck.Height != 0
     && !double.IsPositiveInfinity( toCheck.Height )
     && !double.IsPositiveInfinity( toCheck.Width );

    protected override Size ArrangeOverride( Size finalSize )
    {
        finalSize = base.ArrangeOverride( finalSize );

        if( MapRegion == null )
            return finalSize;

        Clip = GetRegionToViewScalingFactors( new Size( ActualWidth, ActualHeight ) ) != null
            ? new RectangleGeometry { Rect = new Rect( new Point(), new Size( ActualWidth, ActualHeight ) ) }
            : new RectangleGeometry
            {
                Rect = new Rect( new Point(), new Size( MapRegion.VisibleBox.Width, MapRegion.VisibleBox.Height ) )
            };

        ArrangeAnnotations();

        return finalSize;
    }

    private void ArrangeAnnotations()
    {
        if( _annotationsCanvas == null || MapRegion == null )
            return;

        foreach( var element in _annotationsCanvas.Children
                                                  .Where( x => x is FrameworkElement )
                                                  .Cast<FrameworkElement>() )
        {
            Location.TryParseLatLong( element, out var latitude, out var longitude );

            element.PositionRelativeToPoint( MapRegion.GetDisplayPosition( latitude, longitude ) );
        }
    }

    private async Task ShowMessageAsync( string message, string title )
    {
        var mesgBox = new MessageBox { TitleText = title, Text = message, XamlRoot = XamlRoot };

        await mesgBox.ShowAsync();
    }
}
