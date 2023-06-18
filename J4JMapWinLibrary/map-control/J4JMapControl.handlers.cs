using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Shapes;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await InitializeProjectionAsync();
    }

    private void ScaleSliderOnValueChanged( object sender, RangeBaseValueChangedEventArgs e )
    {
        _throttleScaleChanges.Throttle( UpdateEventInterval, _ => MapScale = e.NewValue );
    }

    private void OnSizeChanged( object sender, SizeChangedEventArgs e )
    {
        if( e.NewSize.Width <= 0 || e.NewSize.Height <= 0 )
            return;

        SetMapRectangle();

        _throttleRegionChanges.Throttle( UpdateEventInterval,
                                         async _ => await LoadRegion( (float) e.NewSize.Height,
                                                                      (float) e.NewSize.Width ) );

        _throttleSliderSizeChange.Throttle( UpdateEventInterval, _ => SetControlGridSizes( e.NewSize ) );
    }
}
