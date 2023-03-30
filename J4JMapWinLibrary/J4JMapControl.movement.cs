using System.Linq;
using Microsoft.UI.Xaml.Input;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    private void OnPointerPressed( object sender, PointerRoutedEventArgs e )
    {
        CapturePointer( e.Pointer );
    }

    private void OnPointerMoved( object sender, PointerRoutedEventArgs e )
    {
        if( _movementProcessor == null )
            return;

        if( PointerCaptures?.Any( p => p.PointerId == e.Pointer.PointerId ) ?? false )
            _movementProcessor.AddPoints( e.GetIntermediatePoints( this ) );
    }

    private void OnPointerReleased( object sender, PointerRoutedEventArgs e )
    {
        ReleasePointerCapture( e.Pointer );
    }
}
