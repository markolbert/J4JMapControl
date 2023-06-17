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

        _throttleRegionChanges.Throttle( UpdateEventInterval,
                                         async _ => await LoadRegion( (float) e.NewSize.Height,
                                                                      (float) e.NewSize.Width ) );

        _throttleSliderSizeChange.Throttle( UpdateEventInterval, _ => SetControlGridSizes( e.NewSize ) );
    }

    //private (float Height, float Width) AdjustSizeForViewToRegionScaling( Vector2 size ) =>
    //    AdjustSizeForViewToRegionScaling( new Size( size.X, size.Y ) );

    //private (float Height, float Width) AdjustSizeForViewToRegionScaling( Size newSize )
    //{
    //    if( RegionView == null )
    //        return ( (float) newSize.Height, (float) newSize.Width );

    //    var scalingFactors = GetRegionToViewScalingFactors( newSize );

    //    return scalingFactors != null
    //        ? ( scalingFactors.Value * (float) newSize.Height,
    //            scalingFactors.Value * (float) newSize.Width )
    //        : ( (float) newSize.Height, (float) newSize.Width );
    //}

    //private float? GetRegionToViewScalingFactors( Vector2 size ) =>
    //    GetRegionToViewScalingFactors( new Size( size.X, size.Y ) );

    //private float? GetRegionToViewScalingFactors( Size size )
    //{
    //    if( RegionView == null || ShrinkStyle == ShrinkStyle.None
    //       || size.Height == 0 || size.Width == 0 )
    //        return null;

    //    var center = new Vector3( RegionView.Center.X, RegionView.Center.Y, 0 );

    //    var projWidthHeight = RegionView.Projection.GetHeightWidth((int)MapScale);

    //    var viewRect = new Rectangle2D( (float) size.Height,
    //                                    (float) size.Width,
    //                                    Region.Rotation,
    //                                    center );

    //    var northZoom = GetCornerScalingFactor( viewRect, projWidthHeight, ProjectionSide.North ) ?? float.PositiveInfinity;
    //    var southZoom = GetCornerScalingFactor( viewRect, projWidthHeight, ProjectionSide.South ) ?? float.NegativeInfinity;
    //    var westZoom = GetCornerScalingFactor( viewRect, projWidthHeight, ProjectionSide.West ) ?? float.PositiveInfinity;
    //    var eastZoom = GetCornerScalingFactor( viewRect, projWidthHeight, ProjectionSide.East ) ?? float.NegativeInfinity;

    //    var zooms = CreateFiniteZooms(northZoom,southZoom, westZoom, eastZoom );

    //    return ShrinkStyle switch
    //    {
    //        ShrinkStyle.MaximizeHeight => northZoom < southZoom ? northZoom : southZoom,
    //        ShrinkStyle.MaximizeWidth => westZoom < eastZoom ? westZoom : eastZoom,
    //        ShrinkStyle.PreserveAspectRatio => zooms.Min(),
    //        ShrinkStyle.None => null,
    //        _ => throw new InvalidEnumArgumentException(
    //            $"Unsupported {typeof( ShrinkStyle )} value '{ShrinkStyle}'" )
    //    };

    //    //if( projWidthHeight >= size.Height && projWidthHeight >= size.Width )
    //    //    return null;

    //    //var heightScale = projWidthHeight / (float) size.Height;
    //    //var widthScale = projWidthHeight / (float) size.Width;

    //    //return ShrinkStyle switch
    //    //{
    //    //    MapStretchStyle.FitHeight => ( 1f, heightScale  ),
    //    //    MapStretchStyle.FitWidth => ( widthScale, 1f ),
    //    //    MapStretchStyle.PreserveAspectRatio => heightScale < widthScale
    //    //        ? ( heightScale , heightScale  )
    //    //        : ( widthScale ,widthScale  ),
    //    //    _ => null
    //    //};
    //}

    //public List<float> CreateFiniteZooms( params float[] zooms ) => zooms.Where( float.IsFinite ).ToList();

    //private float? GetCornerScalingFactor( Rectangle2D viewRect, float projHeightWidth, ProjectionSide side )
    //{
    //    if( viewRect.Height <= 0 || viewRect.Width <= 0 )
    //        return null;

    //    Vector3? minMaxCorner = null;
    //    var minMaxValue = side is ProjectionSide.North or ProjectionSide.West ? float.PositiveInfinity : float.NegativeInfinity;

    //    foreach( var corner in viewRect )
    //    {
    //        var curValue = side switch
    //        {
    //            ProjectionSide.North => corner.Y,
    //            ProjectionSide.West => corner.X,
    //            ProjectionSide.South => corner.Y,
    //            ProjectionSide.East => corner.X,
    //            _ => throw new InvalidEnumArgumentException( $"Invalid {typeof( ProjectionSide )} value '{side}'" )
    //        };

    //        // ReSharper disable once InvertIf
    //        if( ( side is ProjectionSide.North or ProjectionSide.West && curValue < minMaxValue )
    //        || ( side is ProjectionSide.South or ProjectionSide.East && curValue > minMaxValue ) )
    //        {
    //            minMaxValue = curValue;
    //            minMaxCorner = corner;
    //        }
    //    }

    //    // should never happen, but
    //    if( minMaxCorner == null )
    //    {
    //        _logger?.LogWarning( "Could not find minMaxCorner" );
    //        return null;
    //    }

    //    var deltaCornerX = minMaxCorner.Value.X - viewRect.Center.X;
    //    var deltaCornerY = minMaxCorner.Value.Y - viewRect.Center.Y;
    //    var slope = deltaCornerY / deltaCornerX;

    //    var centerToCorner = (float) Math.Sqrt( deltaCornerX * deltaCornerX + deltaCornerY * deltaCornerY );

    //    // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
    //    switch( side )
    //    {
    //        case ProjectionSide.North:
    //        case ProjectionSide.South:
    //            var yValue = side == ProjectionSide.North ? -viewRect.Center.Y : projHeightWidth - viewRect.Center.Y;
    //            var xIntersect = viewRect.Center.X + yValue / slope;

    //            return Distance( viewRect.Center, new Vector3( xIntersect, yValue, 0 ) ) / centerToCorner;

    //        case ProjectionSide.West:
    //        case ProjectionSide.East:
    //            var xValue = side == ProjectionSide.West ? -viewRect.Center.X : projHeightWidth - viewRect.Center.X;
    //            var yIntersect = viewRect.Center.Y + slope * xValue;

    //            return Distance( viewRect.Center, new Vector3( xValue, yIntersect, 0 ) ) / centerToCorner;
    //    }

    //    throw new InvalidEnumArgumentException( $"Invalid {typeof( ProjectionSide )} value '{side}'" );
    //}

    //private float Distance( Vector3 pt1, Vector3 pt2 )
    //{
    //    var xDelta = pt2.X - pt1.X;
    //    var yDelta = pt2.Y - pt1.Y;
    //    var zDelta = pt2.Z - pt1.Z;

    //    return (float) Math.Sqrt( xDelta * xDelta + yDelta * yDelta + zDelta * zDelta );
    //}

    private Point ViewPointToRegionPoint( Point point )
    {
        if( RegionView == null || ShrinkStyle == ShrinkStyle.None )
            return point;

        return Zoom == null 
            ? point 
            : new Point( point.X * Zoom.Value, point.Y * Zoom.Value );
    }

    private Point RegionPointToViewPoint(Point point)
    {
        if (RegionView == null || ShrinkStyle == ShrinkStyle.None)
            return point;

        return Zoom == null
            ? point
            : new Point(point.X / Zoom.Value, point.Y / Zoom.Value);
    }

    private void SetImagePanelTransforms()
    {
        if( _mapGrid == null || RegionView == null )
            return;

        // define the transform to move and rotate the grid
        var transforms = new TransformGroup();

        if( Zoom != null )
        {
            transforms.Children.Add(new ScaleTransform
            {
                ScaleX = 1 / Zoom.Value,
                ScaleY = 1 / Zoom.Value
            });
        }

        transforms.Children.Add( new TranslateTransform
        {
            X = Zoom == null
                ? RegionView.LoadedAreaOffset.X
                : RegionView.LoadedAreaOffset.X / Zoom.Value,
            Y = Zoom == null
                ? RegionView.LoadedAreaOffset.Y
                : RegionView.LoadedAreaOffset.Y / Zoom.Value
        } );

        transforms.Children.Add( new RotateTransform
        {
            Angle = MapRotation,
            CenterX = ActualWidth / 2,
            CenterY = ActualHeight / 2
        } );

        _mapGrid.RenderTransform = transforms;
    }

    private void UpdateDisplay()
    {
        LoadMapImages();
        SetImagePanelTransforms();
        IncludeAnnotations();
        IncludePointsOfInterest();
        IncludeRoutes();
    }

    private void LoadMapImages()
    {
        if( _mapGrid == null || RegionView == null || _loadedRegion == null )
            return;

        DefineColumns();
        DefineRows();

        _mapGrid.Children.Clear();

        for( var row = _loadedRegion.FirstRow; row <= _loadedRegion.LastRow; row++ )
        {
            for( var column = _loadedRegion.FirstColumn; column <= _loadedRegion.LastColumn; column++ )
            {
                var block = _loadedRegion.GetBlock(row, column);
                if( block == null )
                    continue;

                var newImage = new Image { Height = block.Height, Width = block.Width };
                Grid.SetColumn( newImage, column );
                Grid.SetRow( newImage, row );

                if( block is { ImageData: not null } )
                {
                    var memStream = new MemoryStream( block.ImageData );

                    var bitmapImage = new BitmapImage();
                    bitmapImage.SetSource( memStream.AsRandomAccessStream() );

                    newImage.Source = bitmapImage;
                }

                _mapGrid.Children.Add( newImage );
            }
        }

        _mapGrid.Translation = _cumlTranslation;
        _mapGrid.Rotation = _cumlRotation;

        InvalidateArrange();
    }

    private void IncludeAnnotations()
    {
        if( _annotationsCanvas == null || RegionView == null )
            return;

        _annotationsCanvas.Children.Clear();

        foreach( var element in Annotations )
        {
            if( !Location.InRegion( element, RegionView ) )
                continue;

            element.Translation = _cumlTranslation;

            element.CenterPoint = CenterPoint;
            element.Rotation = _cumlRotation;

            _annotationsCanvas.Children.Add( element );
        }
    }

    private void IncludePointsOfInterest()
    {
        if( _poiCanvas == null || RegionView == null )
            return;

        _poiCanvas.Children.Clear();

        foreach( var item in _pointsOfInterest.PlacedItems )
        {
            if( item is not PlacedTemplatedElement poiItem
            || !Location.InRegion( item, RegionView )
            || poiItem.VisualElement == null )
                continue;

            var elementOffset = RegionView.GetDisplayPosition( poiItem.Latitude, poiItem.Longitude ) + _cumlTranslation;
            poiItem.VisualElement.PositionRelativeToPoint( elementOffset );

            poiItem.VisualElement.Rotation = _cumlRotation;
            poiItem.VisualElement.CenterPoint = CenterPoint;

            _poiCanvas.Children.Add( poiItem.VisualElement );
        }
    }

    private void IncludeRoutes()
    {
        if( _routesCanvas == null || RegionView == null )
            return;

        _routesCanvas.Children.Clear();

        foreach( var route in MapRoutes )
        {
            if( route.RoutePositions == null )
                continue;

            for( var ptNum = 0; ptNum < route.RoutePositions.PlacedItems.Count; ptNum++ )
            {
                var curPt = route.RoutePositions.PlacedItems[ ptNum ];
                if( !Location.InRegion( curPt, RegionView ) )
                    continue;

                var nextPt = ptNum >= route.RoutePositions.PlacedItems.Count - 1
                    ? null
                    : route.RoutePositions.PlacedItems[ ptNum + 1 ];

                if( curPt is not IPlacedElement curPlaced )
                    continue;

                var curPtPosition = RegionView.GetDisplayPosition( curPlaced.Latitude, curPlaced.Longitude ) + _cumlTranslation;

                if( curPlaced.VisualElement != null )
                    curPtPosition = curPlaced.VisualElement.PositionRelativeToPoint( curPtPosition );

                if( nextPt is not IPlacedElement nextPlaced || nextPlaced.VisualElement == null )
                {
                    if( route.ShowPoints )
                        _routesCanvas.Children.Add( curPlaced.VisualElement );

                    continue;
                }

                var nextPtPosition = RegionView.GetDisplayPosition( nextPlaced.Latitude, nextPlaced.Longitude );

                if( nextPlaced.VisualElement != null )
                    nextPtPosition = nextPlaced.VisualElement.PositionRelativeToPoint( nextPtPosition ) + _cumlTranslation;

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
        if( RegionView == null )
            return;

        var cellWidth = RegionView.ProjectionType switch
        {
            ProjectionType.Static => GridLength.Auto,
            ProjectionType.Tiled => new GridLength( RegionView.Projection.TileHeightWidth ),
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {typeof( ProjectionType )} value '{RegionView.ProjectionType}'" )
        };

        _mapGrid!.ColumnDefinitions.Clear();

        for( var column = 0; column < RegionView.TilesHighWide; column++ )
        {
            _mapGrid.ColumnDefinitions.Add( new ColumnDefinition { Width = cellWidth } );
        }
    }

    private void DefineRows()
    {
        if( RegionView == null ) 
            return;

        var cellHeight = RegionView!.ProjectionType switch
        {
            ProjectionType.Static => GridLength.Auto,
            ProjectionType.Tiled => new GridLength( RegionView.Projection.TileHeightWidth ),
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {typeof( ProjectionType )} value '{RegionView.ProjectionType}'" )
        };

        _mapGrid!.RowDefinitions.Clear();

        for( var row = 0; row < RegionView.TilesHighWide; row++ )
        {
            _mapGrid.RowDefinitions.Add( new RowDefinition { Height = cellHeight } );
        }
    }

    protected override Size MeasureOverride( Size availableSize )
    {
        availableSize = SizeIsValid( availableSize ) ? availableSize : new Size( 100, 100 );

        if( RegionView == null || _projection == null )
            return availableSize;

        var height = Height < availableSize.Height ? Height : availableSize.Height;
        var width = Width < availableSize.Width ? Width : availableSize.Width;

        return new Size( width, height );
    }

    private static bool SizeIsValid( Size toCheck ) =>
        toCheck.Width != 0
     && toCheck.Height != 0
     && !double.IsPositiveInfinity( toCheck.Height )
     && !double.IsPositiveInfinity( toCheck.Width );

    protected override Size ArrangeOverride( Size finalSize )
    {
        finalSize = base.ArrangeOverride( finalSize );

        if( RegionView == null )
            return finalSize;

        Clip = Zoom != null
            ? new RectangleGeometry { Rect = new Rect( new Point(), new Size( ActualWidth, ActualHeight ) ) }
            : new RectangleGeometry
            {
                Rect = new Rect( new Point(), new Size( RegionView.LoadedArea.Width, RegionView.LoadedArea.Height ) )
            };

        ArrangeAnnotations();

        return finalSize;
    }

    private void ArrangeAnnotations()
    {
        if( _annotationsCanvas == null || RegionView == null )
            return;

        foreach( var element in _annotationsCanvas.Children
                                                  .Where( x => x is FrameworkElement )
                                                  .Cast<FrameworkElement>() )
        {
            if( Location.TryParseLatLong( element, out var latitude, out var longitude ) )
                element.PositionRelativeToPoint(
                    RegionView.GetDisplayPosition( latitude, longitude ) + _cumlTranslation );
        }
    }

    private async Task ShowMessageAsync( string message, string title )
    {
        var mesgBox = new MessageBox { TitleText = title, Text = message, XamlRoot = XamlRoot };

        await mesgBox.ShowAsync();
    }
}
