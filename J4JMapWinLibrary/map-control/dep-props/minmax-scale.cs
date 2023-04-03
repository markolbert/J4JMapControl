using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    public DependencyProperty MaxMapScaleProperty = DependencyProperty.Register( nameof( MaxMapScale ),
        typeof( double ),
        typeof( J4JMapControl ),
        new PropertyMetadata( 0.0 ) );

    public DependencyProperty MinMapScaleProperty = DependencyProperty.Register( nameof( MinMapScale ),
        typeof( double ),
        typeof( J4JMapControl ),
        new PropertyMetadata( 0.0 ) );

    //private static void OnMinMaxScaleChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    //{
    //    if( d is not J4JMapControl mapControl )
    //        return;

    //    if( mapControl.MapScale >= mapControl.MinScale && mapControl.MapScale <= mapControl.MaxScale )
    //        return;

    //    mapControl.MapScale = mapControl.MapScale < mapControl.MinScale
    //        ? mapControl.MinScale
    //        : mapControl.MaxScale;
    //}

    public double MinMapScale
    {
        get => (double) GetValue( MinMapScaleProperty );

        private set
        {
            if( value < _projection?.MinScale )
                value = _projection.MinScale;

            SetValue( MinMapScaleProperty, value );

            if( MapScale < value )
                MapScale = value;
        }
    }

    public double MaxMapScale
    {
        get => (double) GetValue( MaxMapScaleProperty );

        private set
        {
            if( value > _projection?.MaxScale )
                value = _projection.MaxScale;

            SetValue( MaxMapScaleProperty, value );

            if( MapScale > value )
                MapScale = value;
        }
    }
}
