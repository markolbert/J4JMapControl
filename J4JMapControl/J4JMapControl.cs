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
        get => (int)GetValue(ZoomLevelProperty);
        set => SetValue(ZoomLevelProperty, value);
    }

    private static void OnZoomLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not J4JMapControl mapControl)
            return;

        mapControl.OnZoomLevelChanged( (int) e.NewValue );
    }

    #endregion

    #region MapRetriever property

    // the IMapImageRetriever being used to display the map layer
    public static readonly DependencyProperty MapRetrieverProperty = DependencyProperty.Register( nameof( MapRetriever ),
        typeof( IMapImageRetriever ),
        typeof( J4JMapControl ),
        new PropertyMetadata( J4JDeusEx.ServiceProvider.GetRequiredService<OpenStreetMapsImageRetriever>(),
                              OnMapImageRetrieverChanged ) );

    public IMapImageRetriever MapRetriever
    {
        get => (IMapImageRetriever)GetValue( MapRetrieverProperty );
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

    public LatLong? Center
    {
        get => (LatLong?) GetValue( CenterProperty );
        set => SetValue( CenterProperty, value );
    }

    private static void OnMapCenterChangedStatic(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if( d is not J4JMapControl mapControl )
            return;

        switch( e.NewValue )
        {
            case null:
                mapControl.OnMapCenterChanged(null);
                break;

            case LatLong latLong:
                mapControl.OnMapCenterChanged( latLong );
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
        _logger?.SetLoggedType(GetType());

        Visibility = Visibility.Collapsed;
    }

    private void OnZoomLevelChanged( int zoom )
    {
        MapRetriever.Zoom!.Level = zoom;
        UpdateMap();
    }

    private void OnMapCenterChanged(LatLong? center)
    {
        if( center == null && Visibility != Visibility.Collapsed )
            Visibility= Visibility.Collapsed;
        else UpdateMap();
    }

    private void UpdateMap()
    {
        if( Center == null || MapRetriever.Zoom == null)
            return;

        Children.Clear();

        var tiles = MapRetriever.GetTileCollection();
        tiles.Update(MapRetriever.Zoom.GetScreenMapRect(Center, Width, Height));

        for( var row = 0; row < tiles.NumRows; row++ )
        {
            for( var col = 0; col < tiles.NumColumns; col++ )
            {
                if( tiles.TryGetTile(row,col, out var coordinates))
                    Children.Add(new TileImage(){Coordinates = coordinates, MapRetriever = MapRetriever}  );
            }
        }

        InvalidateArrange();
    }

    protected override Size MeasureOverride( Size availableSize )
    {
        if( Center == null || MapRetriever.Zoom == null || !Children.Any() )
            return availableSize;

        var tiles = MapRetriever.GetTileCollection();
        var desiredSize = new Size( tiles.ScreenWidth, tiles.ScreenHeight );

        return desiredSize.Width <= availableSize.Width && desiredSize.Height <= availableSize.Height
            ? desiredSize
            : availableSize;
    }

    protected override Size ArrangeOverride( Size finalSize )
    {
        if( Center == null || MapRetriever.Zoom == null || !Children.Any() )
            return finalSize;

        var tiles = MapRetriever.GetTileCollection();
        var desiredSize = new Size(tiles.ScreenWidth, tiles.ScreenHeight);


    }
}