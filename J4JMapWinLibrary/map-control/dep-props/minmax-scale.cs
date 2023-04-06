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
