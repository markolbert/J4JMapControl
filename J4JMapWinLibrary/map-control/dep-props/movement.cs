#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// movement.cs
//
// This file is part of JumpForJoy Software's J4JMapWinLibrary.
// 
// J4JMapWinLibrary is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JMapWinLibrary is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JMapWinLibrary. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.ComponentModel;
using System.Linq;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using J4JSoftware.J4JMapLibrary;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    private readonly MovementProcessor _movementProcessor;
    private Line? _baseLine;
    private Image? _compassRose;
    private float? _firstRotationAngle;
    private Point? _firstRotationPoint;
    private TextBlock? _headingText;
    private Point? _lastTranslationPoint;

    private Canvas? _rotationCanvas;
    private bool _rotationHintsDefined;
    private bool _rotationHintsEnabled;
    private Line? _rotationLine;
    private StackPanel? _rotationPanel;
    private TextBlock? _rotationText;
    private Slider? _scaleSlider;

    public DependencyProperty ShowRotationHintsProperty = DependencyProperty.Register( nameof( ShowRotationHints ),
        typeof( bool ),
        typeof( J4JMapControl ),
        new PropertyMetadata( true ) );

    public bool ShowRotationHints
    {
        get => (bool) GetValue( ShowRotationHintsProperty );
        set => SetValue( ShowRotationHintsProperty, value );
    }

    private void MovementProcessorOnMoved( object? sender, Movement e )
    {
        _rotationHintsEnabled = e.Type == MovementType.Rotation;

        switch( e.Type )
        {
            case MovementType.Rotation:
                ProcessRotation( e.Position );
                break;

            case MovementType.Translation:
                ProcessTranslation( e.Position );
                break;

            case MovementType.Undefined:
            default:
                throw new InvalidEnumArgumentException( $"Unsupported {typeof( MovementType )} value '{e.Type}'" );
        }
    }

    private void MovementProcessorOnMovementsEnded( object? sender, MovementType e )
    {
        switch( e )
        {
            case MovementType.Rotation:
                _rotationHintsEnabled = false;
                _firstRotationAngle = null;
                _firstRotationPoint = null;

                if( _rotationCanvas != null )
                    _rotationCanvas.Visibility = Visibility.Collapsed;

                break;

            case MovementType.Translation:
                _lastTranslationPoint = null;
                break;

            case MovementType.Undefined:
            default:
                // no op
                break;
        }
    }

    private void ProcessRotation( Point point )
    {
        if( !_rotationHintsDefined || !ShowRotationHints || !_rotationHintsEnabled )
            return;

        _firstRotationPoint ??= point;
        _firstRotationAngle ??= (float) this.AngleFromCenter( point );

        var curAngle = (float) this.AngleFromCenter( point );
        var deltaRotation = curAngle - _firstRotationAngle.Value;

        _baseLine!.X1 = ActualWidth / 2;
        _baseLine.Y1 = ActualHeight / 2;
        _baseLine.X2 = _firstRotationPoint.Value.X;
        _baseLine.Y2 = _firstRotationPoint.Value.Y;

        _rotationLine!.X1 = ActualWidth / 2;
        _rotationLine.Y1 = ActualHeight / 2;
        _rotationLine.X2 = point.X;
        _rotationLine.Y2 = point.Y;

        Canvas.SetLeft( _rotationPanel, point.X + 5 );
        Canvas.SetTop( _rotationPanel, point.Y + 5 );
        _rotationText!.Text = $"Rotated {deltaRotation:F0}";

        _headingText!.Text = $"Heading {Heading + deltaRotation:F0}";

        _rotationCanvas!.Visibility = Visibility.Visible;

        // this will trigger an update of the map display
        Heading += deltaRotation;
    }

    private void PositionCompassRose()
    {
        if( _compassRose == null )
            return;

        _compassRose.RenderTransform = new RotateTransform
        {
            Angle = 360 - Heading, CenterX = _compassRose.ActualWidth / 2, CenterY = _compassRose.ActualHeight / 2
        };
    }

    private void ProcessTranslation( Point point )
    {
        _lastTranslationPoint ??= point;

        var xDelta = _lastTranslationPoint.Value.X - point.X;
        var yDelta = _lastTranslationPoint.Value.Y - point.Y;

        if( Math.Abs( xDelta ) < 5 && Math.Abs( yDelta ) < 5 )
            return;

        MapRegion!.Offset( (float) xDelta, (float) yDelta );
        MapRegion!.Update();
    }

    private void OnPointerPressed( object sender, PointerRoutedEventArgs e )
    {
        e.Handled = true;

        CapturePointer( e.Pointer );

        _movementProcessor.Enabled = true;
    }

    private void OnPointerMoved( object sender, PointerRoutedEventArgs e )
    {
        e.Handled = true;

        if( PointerCaptures == null
        || PointerCaptures.All( p => p.PointerId != e.Pointer.PointerId ) )
            return;

        var controlPressed = InputKeyboardSource
                            .GetKeyStateForCurrentThread( VirtualKey.Control )
                            .HasFlag( CoreVirtualKeyStates.Down );

        _movementProcessor.AddPoints( e.GetIntermediatePoints( this ), controlPressed );
    }

    private void OnPointerReleased( object sender, PointerRoutedEventArgs e )
    {
        e.Handled = true;
        _movementProcessor.Enabled = false;

        ReleasePointerCapture( e.Pointer );
    }

    private void MapGridOnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        e.Handled = true;
        var point = e.GetCurrentPoint(this);
        MapScale += point.Properties.MouseWheelDelta < 0 ? -1 : 1;
    }

    public bool SetHeadingByText( string text )
    {
        var heading = text.ToLower().Trim() switch
        {
            "n" => 0D,
            "e" => -90D,
            "s" => 180D,
            "w" => 90D,
            "ne" => -45D,
            "se" => -135D,
            "sw" => 135D,
            "nw" => 45D,
            "nne" => -22.5,
            "ene" => -67.5,
            "ese" => -112.5,
            "sse" => -157.5,
            "ssw" => 157.5,
            "wsw" => 112.5,
            "wnw" => 67.5,
            "nnw" => 22.5,
            _ => double.NaN
        };

        if( double.IsNaN( heading ) )
            return false;

        Heading = heading;

        return true;
    }
}
