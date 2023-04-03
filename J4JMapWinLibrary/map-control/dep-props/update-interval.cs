using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    public DependencyProperty UpdateEventIntervalProperty = DependencyProperty.Register( nameof( UpdateEventInterval ),
        typeof( int ),
        typeof( J4JMapControl ),
        new PropertyMetadata( J4JMapControl.DefaultUpdateEventInterval, OnUpdateIntervalChanged ) );

    private static void OnUpdateIntervalChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
        if( d is not J4JMapControl mapControl )
            return;

        if( e.NewValue is not int value )
            return;

        if( value < 0 )
        {
            mapControl._logger.Warning( "Tried to set UpdateEventInterval < 0, defaulting to {0}",
                                        J4JMapControl.DefaultUpdateEventInterval );
            value = DefaultUpdateEventInterval;
        }

        mapControl.UpdateEventInterval = value;
    }

    public int UpdateEventInterval
    {
        get => (int) GetValue( UpdateEventIntervalProperty );
        set => SetValue( UpdateEventIntervalProperty, value );
    }
}
