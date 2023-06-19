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
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using J4JSoftware.WindowsUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl : Control
{
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

    public int UpdateEventInterval
    {
        get => _updateInterval;
        set => _updateInterval = value < 0 ? DefaultUpdateEventInterval : value;
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _mapGrid = FindUiElement<Grid>("MapGrid", x => x.PointerWheelChanged += MapGridOnPointerWheelChanged);
        _controlGrid = FindUiElement<Grid>("ControlGrid");
        _annotationsCanvas = FindUiElement<Canvas>("AnnotationsCanvas");
        _poiCanvas = FindUiElement<Canvas>("PoICanvas");
        _routesCanvas = FindUiElement<Canvas>("RoutesCanvas");

        _rotationCanvas = FindUiElement<Canvas>("RotationCanvas");
        _rotationPanel = FindUiElement<StackPanel>("RotationPanel");
        _rotationText = FindUiElement<TextBlock>("RotationText");
        _headingText = FindUiElement<TextBlock>("HeadingText");
        _rotationLine = FindUiElement<Line>("RotationLine");
        _baseLine = FindUiElement<Line>("BaseLine");

        _rotationHintsDefined = _rotationCanvas != null
         && _rotationPanel != null
         && _rotationText != null
         && _headingText != null
         && _rotationLine != null
         && _baseLine != null;

        _compassRose = FindUiElement<Image>("CompassRose");
        _scaleSlider = FindUiElement<Slider>("ScaleSlider", x => x.ValueChanged += ScaleSliderOnValueChanged);

        SetMapControlMargins(ControlVerticalMargin);
    }

    private void SetImagePanelTransforms()
    {
        if( _mapGrid == null || _mapRegion == null )
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
                ? _mapRegion.Offset.X
                : _mapRegion.Offset.X / Zoom.Value,
            Y = Zoom == null
                ? _mapRegion.Offset.Y
                : _mapRegion.Offset.Y / Zoom.Value
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
        if( MapRectangle == null )
            return;

        LoadMapImages();
        SetImagePanelTransforms();
        IncludeAnnotations();
        IncludePointsOfInterest();
        IncludeRoutes();
    }

    private void LoadMapImages()
    {
        if( _mapGrid == null || _mapRegion == null )
            return;

        DefineColumns();
        DefineRows();

        _mapGrid.Children.Clear();

        for( var row = _mapRegion.FirstRow; row <= _mapRegion.LastRow; row++ )
        {
            for( var column = _mapRegion.FirstColumn; column <= _mapRegion.LastColumn; column++ )
            {
                var block = _mapRegion.GetBlock(row, column);
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
        if( _annotationsCanvas == null )
            return;

        _annotationsCanvas.Children.Clear();

        foreach( var element in Annotations )
        {
            if( !InRegion( element ) )
                continue;

            element.Translation = _cumlTranslation;

            element.CenterPoint = CenterPoint;
            element.Rotation = _cumlRotation;

            _annotationsCanvas.Children.Add( element );
        }
    }

    private void IncludePointsOfInterest()
    {
        if( _poiCanvas == null )
            return;

        _poiCanvas.Children.Clear();

        foreach( var item in _pointsOfInterest.PlacedItems )
        {
            if( item is not PlacedTemplatedElement poiItem
            || !InRegion( item )
            || poiItem.VisualElement == null )
                continue;

            var elementOffset = GetDisplayPosition(  poiItem.Latitude, poiItem.Longitude )
              + _cumlTranslation;

            poiItem.VisualElement.PositionRelativeToPoint( elementOffset );

            poiItem.VisualElement.Rotation = _cumlRotation;
            poiItem.VisualElement.CenterPoint = CenterPoint;

            _poiCanvas.Children.Add( poiItem.VisualElement );
        }
    }

    private void IncludeRoutes()
    {
        if( _routesCanvas == null )
            return;

        _routesCanvas.Children.Clear();

        foreach( var route in MapRoutes )
        {
            if( route.RoutePositions == null )
                continue;

            for( var ptNum = 0; ptNum < route.RoutePositions.PlacedItems.Count; ptNum++ )
            {
                var curPt = route.RoutePositions.PlacedItems[ ptNum ];
                if( !InRegion( curPt ) )
                    continue;

                var nextPt = ptNum >= route.RoutePositions.PlacedItems.Count - 1
                    ? null
                    : route.RoutePositions.PlacedItems[ ptNum + 1 ];

                if( curPt is not IPlacedElement curPlaced )
                    continue;

                var curPtPosition = GetDisplayPosition( curPlaced.Latitude, curPlaced.Longitude )
                  + _cumlTranslation;

                if( curPlaced.VisualElement != null )
                    curPtPosition = curPlaced.VisualElement.PositionRelativeToPoint( curPtPosition );

                if( nextPt is not IPlacedElement nextPlaced || nextPlaced.VisualElement == null )
                {
                    if( route.ShowPoints )
                        _routesCanvas.Children.Add( curPlaced.VisualElement );

                    continue;
                }

                var nextPtPosition = GetDisplayPosition( nextPlaced.Latitude, nextPlaced.Longitude );

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

    protected override Size MeasureOverride( Size availableSize )
    {
        availableSize = SizeIsValid( availableSize ) ? availableSize : new Size( 100, 100 );

        if( _projection == null )
            return availableSize;

        var height = Height < availableSize.Height ? Height : availableSize.Height;
        var width = Width < availableSize.Width ? Width : availableSize.Width;

        return new Size( width, height );
    }

    protected override Size ArrangeOverride( Size finalSize )
    {
        finalSize = base.ArrangeOverride( finalSize );

        if( MapRectangle == null )
            return finalSize;

        Clip = Zoom != null
            ? new RectangleGeometry { Rect = new Rect( new Point(), new Size( ActualWidth, ActualHeight ) ) }
            : new RectangleGeometry
            {
                Rect = new Rect( new Point(), new Size( MapRectangle.Width, MapRectangle.Height ) )
            };

        ArrangeAnnotations();

        return finalSize;
    }

    private void ArrangeAnnotations()
    {
        if( _annotationsCanvas == null )
            return;

        foreach( var element in _annotationsCanvas.Children
                                                  .Where( x => x is FrameworkElement )
                                                  .Cast<FrameworkElement>() )
        {
            if( Location.TryParseLatLong( element, out var latitude, out var longitude ) )
                element.PositionRelativeToPoint(
                    GetDisplayPosition( latitude, longitude ) + _cumlTranslation );
        }
    }

    private async Task ShowMessageAsync( string message, string title )
    {
        var mesgBox = new MessageBox { TitleText = title, Text = message, XamlRoot = XamlRoot };

        await mesgBox.ShowAsync();
    }

}
