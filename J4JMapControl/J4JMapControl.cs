using System.ComponentModel;
using System.Numerics;
using Windows.Foundation;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.J4JMapControl;

public sealed class J4JMapControl : Panel, IMapContext
{
    #region ZoomLevel property

    // the map's zoom level
    public static readonly DependencyProperty ZoomLevelProperty = DependencyProperty.Register( nameof( Zoom ),
        typeof( int ),
        typeof( J4JMapControl ),
        new PropertyMetadata( J4JDeusEx.ServiceProvider.GetRequiredService<IZoom>(), OnZoomLevelChanged ) );

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
        new PropertyMetadata( J4JDeusEx.ServiceProvider.GetRequiredService<OpenStreetMapsImageRetriever>(),
                              OnMapImageRetrieverChanged ) );

    public IMapImageRetriever MapRetriever
    {
        get => (IMapImageRetriever) GetValue( MapRetrieverProperty );
        set => SetValue( MapRetrieverProperty, value );
    }

    private static void OnMapImageRetrieverChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
        if( d is not J4JMapControl mapControl
        || e.NewValue is not IMapImageRetriever retriever
        || retriever.MapRetrieverInfo == null
        || retriever.Zoom == null )
            return;

        var curLevel = mapControl.MapRetriever.Zoom!.Level;
        mapControl.MapRetriever.Zoom = new Zoom( retriever.MapRetrieverInfo ) { Level = curLevel };
    }

    #endregion

    #region Center property

    // the center of the currently displayed map
    public static readonly DependencyProperty CenterProperty =
        DependencyProperty.Register( "",
                                     typeof( LatLong ),
                                     typeof( J4JMapControl ),
                                     new PropertyMetadata( null, OnMapCenterChangedStatic ) );

    [TypeConverter(typeof(LatLongTypeConverter))]
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

    public J4JMapControl()
    {
        _logger = J4JDeusEx.ServiceProvider.GetRequiredService<IJ4JLogger>();
        _logger?.SetLoggedType( GetType() );

        Visibility = Visibility.Collapsed;
    }

    private async Task OnZoomLevelChanged( int zoom )
    {
        MapRetriever.Zoom!.Level = zoom;
        await UpdateMap();
    }

    private async Task OnMapCenterChanged( LatLong? center )
    {
        if( center == null && Visibility != Visibility.Collapsed )
            Visibility = Visibility.Collapsed;
        else await UpdateMap();
    }

    private async Task UpdateMap()
    {
        if( Center == null || MapRetriever.Zoom == null )
            return;

        var mapRect = MapRetriever.Zoom.GetScreenMapRect( Center, Width, Height );
        var retrievalResult = await MapRetriever.GetMapImagesAsync( mapRect, GetMapImages() );

        if( !retrievalResult.IsValid )
            return;

        var imagesChanged = false;

        foreach( var newImage in retrievalResult.ReturnValue!.Cast<Image>() )
        {
            Children.Add( newImage );
            imagesChanged = true;
        }

        if( imagesChanged )
            InvalidateArrange();
    }

    private List<Image> GetMapImages() =>
        Children.Where( x => x is Image )
                .Cast<Image>()
                .Where( AttachedProperties.GetIsMapTile )
                .ToList();

    protected override Size MeasureOverride( Size availableSize )
    {
        if( Center == null || MapRetriever.Zoom == null || MapRetriever.MapRetrieverInfo == null )
            return availableSize;

        var mapImages = GetMapImages();

        return !mapImages.Any() ? availableSize : GetDesiredMapSize( availableSize, mapImages );
    }

    private Size GetDesiredMapSize( Size availableSize, List<Image> mapImages )
    {
        var coordinates = mapImages.Select( AttachedProperties.GetTileCoordinates ).ToList();

        if( !coordinates.Any() )
            return availableSize;

        if( coordinates.Count == 1 && coordinates[ 0 ] is ScreenGlobalCoordinates )
            return MeasureScreenGlobalCoordinates( availableSize );

        if( coordinates.Any( x => x is ScreenGlobalCoordinates ) )
        {
            _logger?.Error( "Heterogenous mix of Coordinates types, which is unsupported" );
            return availableSize;
        }

        var retVal = new Size( availableSize.Width, availableSize.Height );

        var tiledCoordinates = coordinates.Cast<ScreenTileGlobalCoordinates>()
                                          .ToList();

        var desiredWidth = MapRetriever.MapRetrieverInfo!.DefaultBitmapWidthHeight
          * ( tiledCoordinates.Max( x => x.TileCoordinates.X )
              - tiledCoordinates.Min( x => x.TileCoordinates.X )
              + 1 );

        var desiredHeight = MapRetriever.MapRetrieverInfo.DefaultBitmapWidthHeight
          * ( tiledCoordinates.Max( x => x.TileCoordinates.Y )
              - tiledCoordinates.Min( y => y.TileCoordinates.Y )
              + 1 );

        if( desiredWidth < availableSize.Width )
            retVal.Width = desiredWidth;

        if( desiredHeight < availableSize.Height )
            retVal.Height = desiredHeight;

        return retVal;
    }

    private Size MeasureScreenGlobalCoordinates( Size availableSize )
    {
        var retVal = new Size( availableSize.Width, availableSize.Height );

        if( MapRetriever.MapRetrieverInfo!.DefaultBitmapWidthHeight < retVal.Width )
            retVal.Width = MapRetriever.MapRetrieverInfo.DefaultBitmapWidthHeight;

        if( MapRetriever.MapRetrieverInfo.DefaultBitmapWidthHeight < retVal.Height )
            retVal.Height = MapRetriever.MapRetrieverInfo.DefaultBitmapWidthHeight;

        return retVal;
    }

    protected override Size ArrangeOverride( Size finalSize )
    {
        if( Center == null || MapRetriever.Zoom == null || MapRetriever.MapRetrieverInfo == null )
            return finalSize;

        var mapImages = GetMapImages();
        var desiredSize = GetDesiredMapSize( new Size( double.MaxValue, double.MaxValue ), mapImages );

        // shift everything leftwards by half of the excess (to keep the 
        // map center centered)
        var xOffset = desiredSize.Width <= finalSize.Width ? 0 : ( desiredSize.Width - finalSize.Width ) / 2;
        var yOffset = desiredSize.Height <= finalSize.Height ? 0 : ( desiredSize.Height - finalSize.Height ) / 2;

        foreach( var image in mapImages )
        {
            var imgTileCoord = AttachedProperties.GetTileCoordinates( image );
            if( imgTileCoord == null )
            {
                _logger?.Warning<string>( "Map Image lacks {0}", nameof( AttachedProperties.TileCoordinatesProperty ) );
                continue;
            }

            var centerPt = MapRetriever.Zoom.LatLongToScreen(Center);

            image.CenterPoint = new Vector3((float)centerPt.X - (float)xOffset / 2,
                                            (float)centerPt.Y - (float)yOffset / 2,
                                            0);
        }

        return finalSize;
    }
}