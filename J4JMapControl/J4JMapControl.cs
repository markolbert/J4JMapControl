using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Windows.Foundation;
using Windows.System;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.J4JMapControl;

public sealed partial class J4JMapControl : Panel, IMapContext
{
    private readonly IJ4JLogger? _logger;

    private IMapProjection? _mapProjection;
    private BoundingBox? _boundingBox;
    private bool _pointerCaptured;
    private Point _lastDragPt;
    private GestureRecognizer _gestureRecognizer;

    public J4JMapControl()
    {
        _logger = J4JDeusEx.ServiceProvider.GetRequiredService<IJ4JLogger>();
        _logger?.SetLoggedType( GetType() );

        SizeChanged += ( _, args ) => OnSizeChangedAsync( args );

        _gestureRecognizer = new GestureRecognizer
        {
            GestureSettings = GestureSettings.ManipulationTranslateX 
                              | GestureSettings.ManipulationTranslateY 
                              | GestureSettings.ManipulationScale
                              | GestureSettings.DoubleTap
        };

        _gestureRecognizer.Tapped += GestureRecognizerOnTapped;
        _gestureRecognizer.ManipulationStarted += GestureRecognizerOnManipulationStarted;
        _gestureRecognizer.ManipulationUpdated += GestureRecognizerOnManipulationUpdated;
        _gestureRecognizer.ManipulationCompleted += GestureRecognizerOnManipulationCompleted;

        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        PointerWheelChanged += OnPointerWheelChanged;
    }

    private void GestureRecognizerOnTapped( GestureRecognizer sender, TappedEventArgs args )
    {
        _logger?.Warning("Tapped: ({0}, {1}), tc: {2}", new object[]
        {
            args.Position.X,
            args.Position.Y,
            args.TapCount
        });

        if( args.TapCount < 2 || _mapProjection == null || _boundingBox == null )
            return;

        var centerPt = _boundingBox.UpperLeft.ScreenPoint;
        centerPt.Increment( args.Position.X, args.Position.Y );

        Center = _mapProjection.ScreenToLatLong( centerPt );
    }

    private void GestureRecognizerOnManipulationCompleted( GestureRecognizer sender, ManipulationCompletedEventArgs args )
    {
        _logger?.Warning("Completed: ({0}, {1}), xp: {2}, r: {3}, s: {4}, t: {5}", new object []
        {
            args.Position.X, 
            args.Position.Y, 
            args.Cumulative.Expansion, 
            args.Cumulative.Rotation, 
            args.Cumulative.Scale, 
            args.Cumulative.Translation
        });
    }

    private void GestureRecognizerOnManipulationUpdated( GestureRecognizer sender, ManipulationUpdatedEventArgs args )
    {
        _logger?.Warning("Updated: ({0}, {1}), xp: {2}, r: {3}, s: {4}, t: {5}", new object[]
        {
            args.Position.X,
            args.Position.Y,
            args.Cumulative.Expansion,
            args.Cumulative.Rotation,
            args.Cumulative.Scale,
            args.Cumulative.Translation
        });
    }

    private void GestureRecognizerOnManipulationStarted( GestureRecognizer sender, ManipulationStartedEventArgs args )
    {
        _logger?.Warning("Started: ({0}, {1}), xp: {2}, r: {3}, s: {4}, t: {5}", new object[]
        {
            args.Position.X,
            args.Position.Y,
            args.Cumulative.Expansion,
            args.Cumulative.Rotation,
            args.Cumulative.Scale,
            args.Cumulative.Translation
        });
    }

    #region Map dragging...

    private void OnPointerPressed( object sender, PointerRoutedEventArgs e )
    {
        _gestureRecognizer.ProcessDownEvent( e.GetCurrentPoint( this ) );
        e.Handled = true;

        //if (Center == null || sender is not J4JMapControl mapControl)
        //    return;

        //_pointerCaptured = mapControl.CapturePointer(e.Pointer);

        //if( _pointerCaptured )
        //{
        //    e.Handled = true;
        //    _lastDragPt = e.GetCurrentPoint( this ).Position;
        //}
        //else _logger?.Error("Failed to capture pointer");
    }

    private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        _gestureRecognizer.ProcessUpEvent(e.GetCurrentPoint(this));
        e.Handled = true;

        //if ( !_pointerCaptured || Center == null || sender is not J4JMapControl mapControl)
        //    return;

        //e.Handled = true;

        //OnMapDragged( e.GetCurrentPoint( this ).Position );

        //mapControl.ReleasePointerCapture(e.Pointer);
        //_pointerCaptured = false;
    }

    private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        _gestureRecognizer.ProcessMoveEvents(e.GetIntermediatePoints(this));
        e.Handled = true;

        //if( !_pointerCaptured )
        //    return;

        //e.Handled = true;

        //OnMapDragged(e.GetCurrentPoint(this).Position);
    }

    private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        _gestureRecognizer.ProcessMouseWheelEvent(
            e.GetCurrentPoint(this),
            (e.KeyModifiers & VirtualKeyModifiers.Shift) != 0,
            (e.KeyModifiers & VirtualKeyModifiers.Control) != 0);

        e.Handled = true;
    }

    private void OnMapDragged(Point curDragPt )
    {
        if( !_pointerCaptured || _mapProjection == null || Center == null  )
            return;

        Center = _mapProjection.Offset( Center,
            _lastDragPt.X - curDragPt.X,
            _lastDragPt.Y - curDragPt.Y);

        _lastDragPt = curDragPt;
    }

    #endregion

    #region Property change handlers

    private async Task OnMapImageRetrieverChanged(IMapImageRetriever retriever)
    {
        _mapProjection = new MercatorProjection { MapRetrieverInfo = retriever.MapRetrieverInfo };
        retriever.MapProjection = _mapProjection;

        if( Center == null )
            return;

        await UpdateMap();
    }

    private async Task OnZoomLevelChanged( int zoom )
    {
        if( MapRetriever == null )
            return;

        MapRetriever.MapProjection.ZoomLevel = zoom;

        await UpdateMap();
    }

    private async Task OnMapCenterChanged( LatLong? center )
    {
        if( center == null && Visibility != Visibility.Collapsed )
            Visibility = Visibility.Collapsed;
        else await UpdateMap();
    }

    private async void OnSizeChangedAsync(SizeChangedEventArgs args)
    {
        if( _mapProjection == null || Center == null || args.NewSize.Width > MaxWidth ||
            args.NewSize.Height > MaxHeight )
            return;

        _mapProjection.ViewportWidth = args.NewSize.Width;

        await UpdateMap();
    }

    #endregion

    public List<UIElement> Annotations { get; } = new();

    public async Task UpdateMap()
    {
        if( Center == null || MapRetriever == null || _mapProjection == null )
            return;

        _boundingBox = new BoundingBox( _mapProjection, Center, ActualWidth, ActualHeight );

        var retrievalResult = await MapRetriever
           .GetMapImagesAsync( _boundingBox, Children.MapImages().ExtractCoordinates() );

        if( retrievalResult.ReturnValue == null )
            return;

        Children.SetMapTileState( MapTileState.NotInBoundingBox );

        foreach( var imgData in retrievalResult.ReturnValue!.Cast<MapImageData>() )
        {
            // only add tiles that aren't already in the child collection
            var curImage = Children.GetMapTile( imgData.Coordinates );

            if( curImage != null )
            {
                MapProperties.SetMapTileState(curImage, MapTileState.InUse);
                continue;
            }

            curImage = new Image();

            MapProperties.SetCoordinates( curImage, imgData.Coordinates );
            MapProperties.SetIsMapTile( curImage, true );
            MapProperties.SetIsFixedImageSize( curImage, MapRetriever.FixedSizeImages );
            MapProperties.SetMapTileState(curImage, MapTileState.InUse);

            imgData.Stream.Seek(0);
            var imgSource = new BitmapImage();
            imgSource.SetSource(imgData.Stream);
            curImage.Source = imgSource;

            Children.Add( curImage );
        }

        Children.RemoveMapImages( MapTileState.NotInBoundingBox, MapTileState.NotSet );

        Children.ReplaceAnnotations( Annotations );
    }

    #region Measuring...

    protected override Size MeasureOverride( Size availableSize )
    {
        // not sure why I have to do this guard action, but returning anything
        // with an infinity, or an empty Size, blows up the WinUI library
        if( double.IsPositiveInfinity( availableSize.Width ) )
            availableSize.Width = 100;
        
        if( double.IsPositiveInfinity( availableSize.Height ) )
            availableSize.Height = 100;

        if ( Center == null )
            return availableSize;

        var retVal = MeasureMapLayer( availableSize );

        Clip = new RectangleGeometry { Rect = new Rect(new Point(0, 0), retVal) };

        MeasureAnnotations( retVal );

        return retVal;
    }

    private Size MeasureMapLayer( Size retVal )
    {
        if( _mapProjection == null || _boundingBox == null )
            return retVal;

        double desiredWidth;
        double desiredHeight;

        var varSizedImage = Children.VariableSizedMapImages().FirstOrDefault();

        if( varSizedImage != null )
        {
            // there should only ever by a single variable-sized map image
            var varSize = new Size( ( (BitmapSource)varSizedImage.Source ).PixelWidth,
                ( (BitmapSource)varSizedImage.Source ).PixelHeight );

            desiredWidth = varSize.Width;
            desiredHeight = varSize.Height;

            // map images don't resize -- they stay the same size as when they're created
            varSizedImage.Measure( varSize );
        }
        else
        {
            // process fixed-size map images
            var minXTile = int.MaxValue;
            var minYTile = int.MaxValue;
            var maxXTile = 0;
            var maxYTile = 0;

            foreach( var image in Children.FixedSizedMapImages() )
            {
                // map images don't resize -- they stay the same size as when they're created
                image.Measure( new Size( ( (BitmapSource)image.Source ).PixelWidth,
                    ( (BitmapSource)image.Source ).PixelHeight ) );

                var coords = MapProperties.GetCoordinates( image );
                if( coords == null )
                    continue;

                minXTile = coords.TilePoint.X < minXTile ? coords.TilePoint.X : minXTile;
                minYTile = coords.TilePoint.Y < minYTile ? coords.TilePoint.Y : minYTile;
                maxXTile = coords.TilePoint.X > maxXTile ? coords.TilePoint.X : maxXTile;
                maxYTile = coords.TilePoint.Y > maxYTile ? coords.TilePoint.Y : maxYTile;
            }

            desiredWidth = _mapProjection.TileWidthHeight * ( maxXTile - minXTile + 1 );
            desiredHeight = _mapProjection.TileWidthHeight * ( maxYTile - minYTile + 1 );
        }

        if( desiredWidth < retVal.Width )
            retVal.Width = desiredWidth;

        if( desiredHeight < retVal.Height )
            retVal.Height = desiredHeight;

        MaxWidth = _mapProjection.TileWidthHeight * _boundingBox.HorizontalTiles;
        MaxHeight = _mapProjection.TileWidthHeight * _boundingBox.VerticalTiles;

        return retVal;
    }

    private void MeasureAnnotations( Size clipSize )
    {
        if( _mapProjection == null )
            return;

        foreach (var annotation in Children.GetInitializedAnnotations(clipSize, _mapProjection))
        {
            annotation.Element.Measure( clipSize );
        }
    }

    private double OffsetX()
    {
        // there are two offsets to consider:
        // - the difference between the desired center of the viewport and the actual
        //   center of the tiles that were retrieved
        // - the difference between the size/width of the viewport and the size/width of the tiles
        //   that were retrieved
        var mapOffset = _boundingBox!.GetDesiredCenterOffset( CoordinateAxis.XAxis );
        var vpOffset = ( ActualWidth - _boundingBox.Width ) / 2;

        // if we're displaying everything available horizontally the offset is 0.0
        if ( _boundingBox.HorizontalTiles == _mapProjection!.ZoomFactor )
            return 0.0;

        return vpOffset + mapOffset;
    }

    private double OffsetY()
    {
        // there are two offsets to consider:
        // - the difference between the desired center of the viewport and the actual
        //   center of the tiles that were retrieved
        // - the difference between the size/height of the viewport and the size/height of the tiles
        //   that were retrieved
        var mapOffset = -_boundingBox!.GetDesiredCenterOffset( CoordinateAxis.YAxis );
        var vpOffset = ( ActualHeight - _boundingBox.Height ) / 2;

        // if we're displaying everything available vertically the offset is 0.0
        if ( _boundingBox.VerticalTiles == _mapProjection!.ZoomFactor )
            return 0.0;

        return vpOffset - mapOffset;
    }

    #endregion

    #region Arranging...

    protected override Size ArrangeOverride( Size finalSize )
    {
        if( Center == null
        || MapRetriever == null )
            return finalSize;

        // we do this in layers by starting with arranging the map tiles, and
        // then arranging anything else on top of it
        ArrangeMapTiles();

        ArrangeAnnotations();

        return finalSize;
    }

    private void ArrangeMapTiles()
    {
        if( _mapProjection == null || _boundingBox == null || MapRetriever == null )
            return;

        var xOffset = OffsetX();
        var yOffset = OffsetY();

        foreach ( var image in Children.MapImages() )
        {
            var coords = MapProperties.GetCoordinates( image );
            if( coords == null )
            {
                _logger?.Error<string>( "Map Image lacks {0}", nameof( MapProperties.CoordinatesProperty ) );
                continue;
            }

            var finalRect = MapProperties.GetIsFixedImageSize( image ) 
                ? GetFixedTileRect( coords, xOffset, yOffset ) 
                : null;

            if( finalRect != null )
                image.Arrange( finalRect.Value );
        }
    }

    private Rect? GetFixedTileRect( MultiCoordinates coordinates, double xOffset, double yOffset )
    {
        var (coordsX, coordsY) = coordinates.ScreenPoint.GetValues( CoordinateOrigin.UpperLeft );
        var (boundingX, boundingY) = _boundingBox!.UpperLeft.ScreenPoint.GetValues( CoordinateOrigin.UpperLeft );

        var xPosition = coordsX - boundingX + xOffset;
        var yPosition = coordsY - boundingY + yOffset;

        return new( xPosition,
                    yPosition,
                    _mapProjection!.TileWidthHeight,
                    _mapProjection.TileWidthHeight );
    }

    private void ArrangeAnnotations()
    {
        foreach( var annotation in Children.GetValidAnnotations() )
        {
            annotation.Element.Arrange( new Rect( annotation.AnnotationInfo!.Origin, annotation.Element.DesiredSize ) );
        }
    }

    #endregion
}