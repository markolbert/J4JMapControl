using System;
using System.Linq;
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
        CapturePointer( e.Pointer );
    }

    private void OnPointerMoved( object sender, PointerRoutedEventArgs e )
    {
        if( _movementProcessor == null )
            return;

        if( PointerCaptures?.Any( p => p.PointerId == e.Pointer.PointerId ) ?? false )
            _movementProcessor.AddPoints( e.GetIntermediatePoints( this ), InternalExtensions.IsControlPressed() );
    }

    private void OnPointerReleased( object sender, PointerRoutedEventArgs e )
    {
        ReleasePointerCapture( e.Pointer );
    }

    private void OnRotationHintsStarted(object? sender, EventArgs e)
    {
        _rotationHintsEnabled = true;

        if (_rotationCanvas != null)
            _rotationCanvas.Visibility = Visibility.Visible;
    }

    private void OnRotationHint(object? sender, RotationInfo info)
    {
        if (!_rotationHintsDefined || !ShowRotationHints || !_rotationHintsEnabled)
            return;

        Canvas.SetLeft(_rotationText, info.CurrentTip.Position.X + 5);
        Canvas.SetTop(_rotationText, info.CurrentTip.Position.Y + 5);
        _rotationText!.Text = info.Rotation.ToString("F0");

        _baseLine!.X1 = ActualWidth / 2;
        _baseLine.Y1 = ActualHeight / 2;
        _baseLine.X2 = info.FirstTip.Position.X;
        _baseLine.Y2 = info.FirstTip.Position.Y;

        _rotationLine!.X1 = ActualWidth / 2;
        _rotationLine.Y1 = ActualHeight / 2;
        _rotationLine.X2 = info.CurrentTip.Position.X;
        _rotationLine.Y2 = info.CurrentTip.Position.Y;
    }

    private void OnRotationHintsEnded(object? sender, EventArgs e)
    {
        _rotationHintsEnabled = false;

        if( _rotationCanvas != null )
            _rotationCanvas.Visibility = Visibility.Collapsed;
    }

}
