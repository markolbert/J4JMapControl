using J4JSoftware.J4JMapLibrary;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    public DependencyProperty HeadingProperty = DependencyProperty.Register( nameof( Heading ),
                                                                             typeof( double ),
                                                                             typeof( J4JMapControl ),
                                                                             new PropertyMetadata(
                                                                                 0D,
                                                                                 OnHeadingChanged ) );

    private static void OnHeadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not J4JMapControl mapControl)
            return;

        if (e.NewValue is not double heading)
            return;

        mapControl.MapRegion!.Heading((float)heading);
    }

    public DependencyProperty ShowRotationHintsProperty = DependencyProperty.Register( nameof( ShowRotationHints ),
        typeof( bool ),
        typeof( J4JMapControl ),
        new PropertyMetadata( true ) );
}
