using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    public DependencyProperty UpdateEventIntervalProperty = DependencyProperty.Register( nameof( UpdateEventInterval ),
        typeof( int ),
        typeof( J4JMapControl ),
        new PropertyMetadata( DefaultUpdateEventInterval, OnUpdateIntervalChanged ) );

    public int UpdateEventInterval
    {
        get => (int) GetValue( UpdateEventIntervalProperty );
        set => SetValue( UpdateEventIntervalProperty, value );
    }

    private static void OnUpdateIntervalChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
        if( d is not J4JMapControl mapControl )
            return;

        if( e.NewValue is not int value )
            return;

        if( value < 0 )
        {
            mapControl._logger?.LogWarning( "Tried to set UpdateEventInterval < 0, defaulting to {0}",
                                            DefaultUpdateEventInterval );
            value = DefaultUpdateEventInterval;
        }

        mapControl.UpdateEventInterval = value;
    }
}
