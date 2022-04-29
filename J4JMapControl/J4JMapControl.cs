using System.Runtime.CompilerServices;
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

        mapControl.Zoom.Level = (int)e.NewValue;
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
        || retriever.MapRetrieverInfo == null )
            return;

        var curLevel = mapControl.Zoom.Level;
        mapControl.Zoom = new Zoom( retriever.MapRetrieverInfo ) { Level = curLevel };

        var temp = retriever.GetTileCollection();
     
        if( temp is not ITileCollection tileCollection )
            mapControl._logger?.Error( "{0} return a {1} instead of a {2}",
                                       nameof( IMapImageRetriever.GetTileCollection ),
                                       temp.GetType(),
                                       typeof( ITileCollection ) );
        else mapControl.MapTiles = tileCollection;

        mapControl.UpdateMap?.Invoke( mapControl, EventArgs.Empty );
    }

    #endregion

    #region Center property

    // the center of the currently displayed map
    public static readonly DependencyProperty CenterProperty =
        DependencyProperty.Register( "",
                                     typeof( MapPoint ),
                                     typeof( J4JMapControl ),
                                     new PropertyMetadata( null, OnMapCenterChangedStatic ) );

    public MapPoint Center
    {
        get => (MapPoint)GetValue( CenterProperty );
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

            case MapPoint mapPoint:
                mapControl.OnMapCenterChanged( mapPoint );
                break;

            default:
                mapControl._logger?.Error( "{0} received a {1} instead of a {2}",
                                           nameof( OnMapCenterChangedStatic ),
                                           e.NewValue.GetType(),
                                           typeof( MapPoint ) );
                break;
        }
    }

    #endregion

    public event EventHandler? UpdateMap;

    private readonly IJ4JLogger? _logger;

    public J4JMapControl()
    {
        _logger = J4JDeusEx.ServiceProvider.GetRequiredService<IJ4JLogger>();
        _logger?.SetLoggedType(GetType());

        // we only respond to zoom changes which are valid, i.e., within
        // the range for the current map retriever
        Zoom = J4JDeusEx.ServiceProvider.GetRequiredService<IZoom>();
        Zoom.Changed += OnZoomLevelChanged;

        Visibility = Visibility.Collapsed;
    }

    private void OnZoomLevelChanged( object? sender, int e )
    {
        UpdateMap?.Invoke(this, EventArgs.Empty  );
    }

    private void OnMapCenterChanged(MapPoint? center)
    {
        if( center == null )
        {
            Visibility= Visibility.Collapsed;
            return;
        }

        UpdateMap?.Invoke(this, EventArgs.Empty);
    }

    public ITileCollection? MapTiles { get; private set; }

    public IZoom Zoom { get; private set; }
}