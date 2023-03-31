using System;
using System.Linq;
using Windows.System;
using Windows.UI.Core;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    public bool ShowRotationHints
    {
        get => (bool) GetValue( ShowRotationHintsProperty );
        set => SetValue( ShowRotationHintsProperty, value );
    }

    private void OnPointerPressed( object sender, PointerRoutedEventArgs e )
    {
        if (_movementProcessor == null)
            return;

        CapturePointer( e.Pointer );
        _movementProcessor.Enabled = true;
    }

    private void OnPointerMoved( object sender, PointerRoutedEventArgs e )
    {
        if( _movementProcessor == null
        || PointerCaptures == null
        || PointerCaptures.All( p => p.PointerId != e.Pointer.PointerId ) )
            return;

        var controlPressed = InputKeyboardSource
                            .GetKeyStateForCurrentThread( VirtualKey.Control )
                            .HasFlag( CoreVirtualKeyStates.Down );

        _movementProcessor.AddPoints( e.GetIntermediatePoints( this ), controlPressed );
    }

    private void OnPointerReleased( object sender, PointerRoutedEventArgs e )
    {
        if (_movementProcessor == null)
            return;

        _movementProcessor.Enabled = false;

        ReleasePointerCapture( e.Pointer );
    }

    private void OnRotationHintsStarted(object? sender, EventArgs e)
    {
        _rotationHintsEnabled = true;
    }

    private void OnRotationHint( object? sender, RotationInfo info )
    {
        if( !_rotationHintsDefined || !ShowRotationHints || !_rotationHintsEnabled )
            return;

        _baseLine!.X1 = ActualWidth / 2;
        _baseLine.Y1 = ActualHeight / 2;
        _baseLine.X2 = info.FirstTip.Position.X;
        _baseLine.Y2 = info.FirstTip.Position.Y;

        _rotationLine!.X1 = ActualWidth / 2;
        _rotationLine.Y1 = ActualHeight / 2;
        _rotationLine.X2 = info.CurrentTip.Position.X;
        _rotationLine.Y2 = info.CurrentTip.Position.Y;

        Canvas.SetLeft( _rotationText, info.CurrentTip.Position.X + 5 );
        Canvas.SetTop( _rotationText, info.CurrentTip.Position.Y + 5 );

        _rotationText!.Text = info.Rotation.ToString( "F0" );

        _rotationCanvas!.Visibility = Visibility.Visible;
    }

    private void OnRotationHintsEnded(object? sender, EventArgs e)
    {
        _rotationHintsEnabled = false;

        if( _rotationCanvas != null )
            _rotationCanvas.Visibility = Visibility.Collapsed;
    }

}
