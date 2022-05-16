using J4JSoftware.MapLibrary;
using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapControl;

public sealed partial class J4JMapControl
{
    #region ZoomLevel property

    // the map's zoom level
    public static readonly DependencyProperty ZoomLevelProperty = DependencyProperty.Register( nameof( ZoomLevel ),
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

    private static async void OnMapImageRetrieverChangedStatic(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
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
        DependencyProperty.Register( nameof( Center ),
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

}
