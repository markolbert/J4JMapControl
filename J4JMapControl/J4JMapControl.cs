using Windows.Foundation;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.J4JMapControl;

public sealed class J4JMapControl : Panel, IMapContext
{
    #region ZoomLevel property

    // the map's zoom level
    public static readonly DependencyProperty ZoomLevelProperty = DependencyProperty.Register( nameof( MercatorProjection ),
        typeof( int ),
        typeof( J4JMapControl ),
        new PropertyMetadata( 1, OnZoomLevelChanged ) );

    public int ZoomLevel
    {
        get => (int) GetValue( ZoomLevelProperty );
        set => SetValue( ZoomLevelProperty, value );
    }

    private static async void OnZoomLevelChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
        if( d is not J4JMapControl mapControl )
            return;

        await mapControl.OnZoomLevelChanged( (int) e.NewValue );
    }

    #endregion

    #region MapRetriever property

    // the IMapImageRetriever being used to display the map layer
    public static readonly DependencyProperty MapRetrieverProperty = DependencyProperty.Register(
        nameof( MapRetriever ),
        typeof( IMapImageRetriever ),
        typeof( J4JMapControl ),
        new PropertyMetadata( null, OnMapImageRetrieverChangedStatic ) );

    public IMapImageRetriever? MapRetriever
    {
        get => (IMapImageRetriever?) GetValue( MapRetrieverProperty );
        set => SetValue( MapRetrieverProperty, value );
    }

    private static async void OnMapImageRetrieverChangedStatic( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
        if( d is not J4JMapControl mapControl
        || mapControl.MapRetriever == null
        || e.NewValue is not IMapImageRetriever retriever )
            return;

        await mapControl.OnMapImageRetrieverChanged( retriever );
    }

    #endregion

    #region Center property

    // the center of the currently displayed map
    public static readonly DependencyProperty CenterProperty =
        DependencyProperty.Register( "",
                                     typeof( LatLong ),
                                     typeof( J4JMapControl ),
                                     new PropertyMetadata( null, OnMapCenterChangedStatic ) );

    public LatLong? Center
    {
        get => (LatLong?) GetValue( CenterProperty );
        set => SetValue( CenterProperty, value );
    }

    private static async void OnMapCenterChangedStatic( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
        if( d is not J4JMapControl mapControl )
            return;

        switch( e.NewValue )
        {
            case null:
                await mapControl.OnMapCenterChanged( null );
                break;

            case LatLong latLong:
                await mapControl.OnMapCenterChanged( latLong );
                break;

            default:
                mapControl._logger?.Error( "{0} received a {1} instead of a {2}",
                                           nameof( OnMapCenterChangedStatic ),
                                           e.NewValue.GetType(),
                                           typeof( LatLong ) );
                break;
        }
    }

    #endregion

    private readonly IJ4JLogger? _logger;

    private IMapProjection? _mapProjection;
    private int _tileWidthHeight;
    private BoundingBox? _boundingBox;

    public J4JMapControl()
    {
        _logger = J4JDeusEx.ServiceProvider.GetRequiredService<IJ4JLogger>();
        _logger?.SetLoggedType( GetType() );

        SizeChanged += ( _, args ) => OnSizeChangedAsync( args );
    }

    private async Task OnMapImageRetrieverChanged(IMapImageRetriever retriever)
    {
        _mapProjection = new MercatorProjection { MapRetrieverInfo = retriever.MapRetrieverInfo };
        retriever.MapProjection = _mapProjection;

        _tileWidthHeight = retriever.MapRetrieverInfo.DefaultBitmapWidthHeight;

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
        if( _mapProjection == null || Center == null )
            return;

        _mapProjection.ViewportWidth = args.NewSize.Width;

        await UpdateMap();
    }

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
                AttachedProperties.SetMapTileState(curImage, MapTileState.InUse);
                continue;
            }

            curImage = new Image();

            AttachedProperties.SetCoordinates( curImage, imgData.Coordinates );
            AttachedProperties.SetIsMapTile( curImage, true );
            AttachedProperties.SetIsFixedImageSize( curImage, MapRetriever.FixedSizeImages );
            AttachedProperties.SetMapTileState(curImage, MapTileState.InUse);

            imgData.Stream.Seek(0);
            var imgSource = new BitmapImage();
            imgSource.SetSource(imgData.Stream);
            curImage.Source = imgSource;

            Children.Add( curImage );
        }

        Children.RemoveMapImages( MapTileState.NotInBoundingBox, MapTileState.NotSet );
    }

    protected override Size MeasureOverride( Size availableSize )
    {
        // not sure why I have to do this guard action, but returning anything
        // with an infinity, or an empty Size, blows up the WinUI library
        if( double.IsPositiveInfinity( availableSize.Width ) )
            availableSize.Width = 100;
        
        if( double.IsPositiveInfinity( availableSize.Height ) )
            availableSize.Height = 100;

        if ( Center == null || !Children.OfType<Image>().Where( AttachedProperties.GetIsMapTile ).Any() )
            return availableSize;

        return MeasureMapLayer( availableSize );
    }

    private Size MeasureMapLayer( Size retVal )
    {
        var minXTile = int.MaxValue;
        var minYTile = int.MaxValue;
        var maxXTile = 0;
        var maxYTile = 0;

        var varSizedImage = Children.VariableSizedMapImages().FirstOrDefault();

        if( varSizedImage != null )
        {
            // there should only ever by a single variable-sized map image
            minXTile = 0;
            minYTile = 0;

            // map images don't resize -- they stay the same size as when they're created
            varSizedImage.Measure( new Size( ( (BitmapSource) varSizedImage.Source ).PixelWidth,
                                             ( (BitmapSource) varSizedImage.Source ).PixelHeight ) );
        }
        else
        {
            // process fixed-size map images
            foreach( var image in Children.FixedSizedMapImages() )
            {
                // map images don't resize -- they stay the same size as when they're created
                image.Measure( new Size( ( (BitmapSource) image.Source ).PixelWidth,
                                         ( (BitmapSource) image.Source ).PixelHeight ) );

                var coords = AttachedProperties.GetCoordinates( image );
                if( coords == null )
                    continue;

                minXTile = coords.TilePoint.X < minXTile ? coords.TilePoint.X : minXTile;
                minYTile = coords.TilePoint.Y < minYTile ? coords.TilePoint.Y : minYTile;
                maxXTile = coords.TilePoint.X > maxXTile ? coords.TilePoint.X : maxXTile;
                maxYTile = coords.TilePoint.Y > maxYTile ? coords.TilePoint.Y : maxYTile;
            }
        }

        var desiredWidth = _tileWidthHeight * ( maxXTile - minXTile + 1 );
        var desiredHeight = _tileWidthHeight * ( maxYTile - minYTile + 1 );

        if( desiredWidth < retVal.Width )
            retVal.Width = desiredWidth;

        if( desiredHeight < retVal.Height )
            retVal.Height = desiredHeight;

        return retVal;
    }

    protected override Size ArrangeOverride( Size finalSize )
    {
        if( Center == null
        || MapRetriever == null
        || _tileWidthHeight == 0
        || !Children.MapImages().Any() )
            return finalSize;

        Clip = new RectangleGeometry { Rect = new Rect( new Point( 0, 0 ), finalSize ) };

        // we do this in layers by starting with arranging the map tiles, and
        // then arranging anything else on top of it
        ArrangeMapTiles( finalSize );

        return finalSize;
    }

    private void ArrangeMapTiles( Size finalSize )
    {
        if( _mapProjection == null || _boundingBox == null )
            return;

        // determine if we need to shift the images
        var xOffset = 0.0;

        if( _boundingBox.HorizontalTiles > 1 && _boundingBox.Width > finalSize.Width )
            xOffset = ( finalSize.Width - _boundingBox.Width ) / 2;

        var yOffset = 0.0;

        if( _boundingBox.VerticalTiles > 1 && _boundingBox.Height > finalSize.Height )
            yOffset = ( finalSize.Height - _boundingBox.Height ) / 2;


        foreach ( var image in Children.MapImages() )
        {
            var coords = AttachedProperties.GetCoordinates( image );
            if( coords == null )
            {
                _logger?.Warning<string>( "Map Image lacks {0}", nameof( AttachedProperties.CoordinatesProperty ) );
                continue;
            }

            Rect? finalRect;

            if( AttachedProperties.GetIsFixedImageSize( image ) )
            {
                var coordinates = AttachedProperties.GetCoordinates( image );
                if( coordinates == null )
                    continue;

                var upperLeftX = coordinates.ScreenPoint.GetX( CoordinateOrigin.UpperLeft )
                  - _boundingBox.UpperLeft.ScreenPoint.GetX( CoordinateOrigin.UpperLeft );

                var upperLeftY = coordinates.ScreenPoint.GetY( CoordinateOrigin.UpperLeft )
                  - _boundingBox.UpperLeft.ScreenPoint.GetY( CoordinateOrigin.UpperLeft );

                finalRect = new( upperLeftX + xOffset,
                                 upperLeftY + yOffset,
                                 _tileWidthHeight,
                                 _tileWidthHeight );
            }
            else finalRect = null;

            if( finalRect != null )
                image.Arrange( finalRect.Value );
        }
    }
}