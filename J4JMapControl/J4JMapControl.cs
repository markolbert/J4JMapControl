using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
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

public sealed partial class J4JMapControl : Panel
{
    private const float FloatTolerance = 0.1F;
    private const double DoubleTolerance = 0.1;
    private const double TranslationToScale = 20.0;

    private readonly GestureRecognizer _gestureRecognizer;
    private readonly IJ4JLogger? _logger;

    private IMapProjection? _mapProjection;
    private BoundingBox? _boundingBox;
    private VirtualKeyModifiers _keyModifiers;
    private double _lastScaleOffset;
    private Point _gestureInitialPoint;
    private double _gestureInitialAngle;

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
        if( args.TapCount < 2 || _mapProjection == null || _boundingBox == null || Center == null )
            return;

        var deltaX = args.Position.X - _boundingBox.Viewport.Width / 2;

        // remember, in control space increasing Y values take you >>down<< the page but
        // in projection space they take you >>up<< the page...so you have to invert the
        // delta
        var deltaY = _boundingBox.Viewport.Height / 2 - args.Position.Y;

        Center = _mapProjection.Offset(Center, deltaX, deltaY);
    }

    private void GestureRecognizerOnManipulationCompleted( GestureRecognizer sender, ManipulationCompletedEventArgs args )
    {
    }

    private void GestureRecognizerOnManipulationUpdated( GestureRecognizer sender, ManipulationUpdatedEventArgs args )
    {
        if( _mapProjection == null || Center == null )
            return;

        if( Math.Abs( args.Delta.Scale - 1 ) > FloatTolerance )
            OnScaleChanged( args.Delta.Scale );

        if( Math.Abs( args.Delta.Rotation - 0 ) > FloatTolerance )
            OnMapRotationChanged( args.Delta.Rotation );

        // ignore minor translations
        if (Math.Abs(args.Delta.Translation.X - 1.0) < DoubleTolerance
         || Math.Abs(args.Delta.Translation.Y - 1.0) < DoubleTolerance)
            return;

        // we respond to translation events differently if certain
        // keyboard modifiers are in play
        if (_keyModifiers.HasFlag(VirtualKeyModifiers.Shift))
        {
            OnScaleChanged(args.Cumulative.Translation.X);
            return;
        }

        if (_keyModifiers.HasFlag(VirtualKeyModifiers.Control))
        {
            // rotation: translation represents the vector applied to the original position
            // to get to where we are now
            var curPt = new Point( _gestureInitialPoint.X + args.Cumulative.Translation.X,
                                   _gestureInitialPoint.Y + args.Cumulative.Translation.Y );

            var curAngle = curPt.Y != 0
                ? Math.Atan2(curPt.Y, curPt.X).RadiansToDegrees()
                : 0.0;

            OnMapRotationChanged(Convert.ToSingle(curAngle - _gestureInitialAngle));

            return;
        }

        // if no modification keys were detected this must be a basic translation,
        // so move the center in the direction opposite to what deltaTranslation reports
        // so that the map tracks the pointer. 

        // remember, in control space increasing Y values take you >>down<< the page but
        // in projection space they take you >>up<< the page...so you have to invert the
        // delta, and since we are inverting twice we just pass the value thru
        Center = _mapProjection.Offset(Center, -args.Delta.Translation.X, args.Delta.Translation.Y);
    }

    private void OnScaleChanged( double cumulative )
    {
        // ignore small changes until they accumulate
        if( Math.Abs( cumulative - _lastScaleOffset ) < TranslationToScale )
            return;

        var scaleChange = Convert.ToInt32((cumulative - _lastScaleOffset) / TranslationToScale);
        _lastScaleOffset += scaleChange * TranslationToScale;

        ZoomLevel += scaleChange;
    }

    private void OnMapRotationChanged( float deltaRotation )
    {
        if( Math.Abs( deltaRotation ) < 1F )
            return;

        _logger?.Warning( "Rotation: {0}", deltaRotation );
    }

    private void GestureRecognizerOnManipulationStarted( GestureRecognizer sender, ManipulationStartedEventArgs args )
    {
        _lastScaleOffset = 0.0;
    }

    #region Pointer event handlers...

    private void OnPointerPressed( object sender, PointerRoutedEventArgs e )
    {
        if( _boundingBox == null )
            return;

        _keyModifiers = e.KeyModifiers;

        // get initial point relative to viewport center for detecting
        // rotation motions using single-focus gestures (e.g., mouse, pen)
        // this point's coordinates are relative to the center of the viewport
        _gestureInitialPoint = e.GetCurrentPoint( this ).Position;
        _gestureInitialPoint.X -= _boundingBox.Viewport.Width / 2.0;
        _gestureInitialPoint.Y -= _boundingBox.Viewport.Height / 2.0;

        _gestureInitialAngle = _gestureInitialPoint.Y != 0
            ? Math.Atan2( _gestureInitialPoint.Y, _gestureInitialPoint.X )
                  .RadiansToDegrees()
            : 0.0;

        _gestureRecognizer.ProcessDownEvent( e.GetCurrentPoint( this ) );
        e.Handled = true;
    }

    private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (_boundingBox == null)
            return;

        _gestureRecognizer.ProcessUpEvent(e.GetCurrentPoint(this));
        e.Handled = true;
    }

    private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_boundingBox == null)
            return;

        _gestureRecognizer.ProcessMoveEvents(e.GetIntermediatePoints(this));
        e.Handled = true;
    }

    private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        _gestureRecognizer.ProcessMouseWheelEvent(
            e.GetCurrentPoint(this),
            (e.KeyModifiers & VirtualKeyModifiers.Shift) != 0,
            (e.KeyModifiers & VirtualKeyModifiers.Control) != 0);

        e.Handled = true;
    }

    #endregion

    #region Property change handlers

    private async Task OnMapImageRetrieverChanged(IMapImageRetriever retriever)
    {
        _mapProjection = new MercatorProjection { MapRetrieverInfo = retriever.MapRetrieverInfo! };
        retriever.MapProjection = _mapProjection;

        if( Center == null )
            return;

        await UpdateMap();
    }

    private async Task OnZoomLevelChanged( int zoom )
    {
        if( MapRetriever?.MapRetrieverInfo == null )
            return;

        if( zoom >= MapRetriever.MapRetrieverInfo.MinimumZoom
        && zoom <= MapRetriever.MapRetrieverInfo.MaximumZoom )
        {
            MapRetriever.MapProjection.ZoomLevel = zoom;
            _logger?.Warning( "Changed zoom to {0}", zoom );
            await UpdateMap();
        }
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

        //var junk = _mapProjection.GetBoundingBox(Center, ActualWidth, ActualHeight, 0.0);
        _boundingBox = _mapProjection.GetBoundingBox( Center, ActualWidth, ActualHeight, (double) GetValue(MapRotationProperty) );

        var retrievalResult = await MapRetriever
           .GetMapImagesAsync( _boundingBox, Children.MapImages().ExtractMapTiles() );

        // determine which existing tiles are still in the bounding box area
        foreach( var image in Children.MapImages() )
        {
            // this test should never fail (belt-and-suspenders)
            var mapTile = MapProperties.GetTile( image );
            if( mapTile == null )
            {
                MapProperties.SetMapTileState(image, MapTileState.NotAMapTile);
                continue;
            }

            MapProperties.SetMapTileState( image,
                                           _boundingBox.TileRegion.IsInRegion( mapTile )
                                               ? MapTileState.InUse
                                               : MapTileState.NotInBoundingBox );
        }

        foreach( var imgData in retrievalResult.ReturnValue ?? Enumerable.Empty<MapImageData>() )
        {
            // only add tiles that aren't already in the child collection
            // (we should never find any because they should've been excluded when we
            // retrieved the tile images, so this is a belt-and-suspenders check)
            var curImage = Children.GetMapTile( imgData.MapTile );

            if( curImage != null )
            {
                MapProperties.SetMapTileState(curImage, MapTileState.InUse);
                continue;
            }

            curImage = new Image();

            MapProperties.SetTile( curImage, imgData.MapTile );
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

                var tile = MapProperties.GetTile( image );
                if( tile == null )
                    continue;

                minXTile = tile.X < minXTile ? tile.X : minXTile;
                minYTile = tile.Y < minYTile ? tile.Y : minYTile;
                maxXTile = tile.X > maxXTile ? tile.X : maxXTile;
                maxYTile = tile.Y > maxYTile ? tile.Y : maxYTile;
            }

            desiredWidth = _mapProjection.TileWidthHeight * ( maxXTile - minXTile + 1 );
            desiredHeight = _mapProjection.TileWidthHeight * ( maxYTile - minYTile + 1 );
        }

        if( desiredWidth < retVal.Width )
            retVal.Width = desiredWidth;

        if( desiredHeight < retVal.Height )
            retVal.Height = desiredHeight;

        MaxWidth = _mapProjection.TileWidthHeight * _boundingBox.TileRegion.HorizontalTiles;
        MaxHeight = _mapProjection.TileWidthHeight * _boundingBox.TileRegion.VerticalTiles;

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

        foreach ( var image in Children.MapImages() )
        {
            if( !MapProperties.GetIsMapTile( image ) )
                continue;

            var tile = MapProperties.GetTile(image);
            if (tile == null)
            {
                _logger?.Error<string>("Map Image lacks {0}", nameof(MapProperties.TileProperty));
                continue;
            }

            if ( MapProperties.GetIsFixedImageSize( image ) )
                ArrangeFixedMapTile( image, tile );
            else ArrangeVariableMapTile( image, tile );
        }
    }

    private void ArrangeFixedMapTile( Image image, MapTile mapTile )
    {
        if (_mapProjection == null || _boundingBox == null)
            return;

        var tilePoint = _mapProjection.MapTileToCartesian(mapTile);
        var upperLeftPoint = _mapProjection.MapTileToCartesian(_boundingBox.TileRegion.UpperLeft);
        var offset = _boundingBox.GetOffset();

        var xTranslation = tilePoint.X - upperLeftPoint.X + offset.X;
        var yTranslation = tilePoint.Y - upperLeftPoint.Y + offset.Y;

        var projectionCenter = _mapProjection.GetProjectionRegion( _boundingBox ).Center();
        var tileProjectionCenter = _mapProjection.MapTileCenterToCartesian( mapTile );

        var compTransform = new CompositeTransform
        {
            TranslateX = xTranslation, 
            TranslateY = yTranslation,
            CenterX = projectionCenter.X - tileProjectionCenter.X,
            CenterY = tileProjectionCenter.Y - projectionCenter.Y,
            Rotation = MapRotation
        };

        var imageRect = compTransform.TransformBounds( new Rect( 0.0,
                                                                 0.0,
                                                                 _mapProjection.TileWidthHeight,
                                                                 _mapProjection.TileWidthHeight ) );

        image.Arrange( imageRect );
    }

    private void ArrangeVariableMapTile( Image image, MapTile mapTile )
    {
    }

    private Rect? GetFixedTileRect( MapTile mapTile, Point offset )
    {
        if( _mapProjection == null || _boundingBox == null )
            return null;

        var tilePoint = _mapProjection.MapTileToCartesian( mapTile );
        var upperLeftPoint = _mapProjection.MapTileToCartesian( _boundingBox.TileRegion.UpperLeft );

        var xPosition = tilePoint.X - upperLeftPoint.X + offset.X;
        var yPosition = tilePoint.Y - upperLeftPoint.Y + offset.Y;

        //var xPosition = coordinates.ScreenPoint.X - _boundingBox!.UpperLeft.ScreenPoint.X + xOffset;
        //var yPosition = coordinates.ScreenPoint.Y - _boundingBox.UpperLeft.ScreenPoint.Y + yOffset;

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