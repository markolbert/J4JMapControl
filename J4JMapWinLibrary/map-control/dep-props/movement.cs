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
using J4JSoftware.WindowsUtilities;
using Microsoft.Extensions.Logging;
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
    private readonly ThrottleDispatcher _throttleDoubleTap = new();

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

        SetValue( CenterProperty,
                  MapExtensions.ConvertToLatLongText( MapRegion.CenterLatitude, MapRegion.CenterLongitude ) );
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

    private void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        _throttleDoubleTap.Throttle( UpdateEventInterval, _ => CenterOnDoubleTap( e.GetPosition( this ) ) );
        e.Handled = true;
    }

    private void CenterOnDoubleTap( Point point )
    {
        var xOffset = point.X - ActualWidth / 2;
        var yOffset = point.Y - ActualHeight / 2;

        MapRegion!.Offset((float)xOffset, (float)yOffset);
    }

    public bool SetHeadingByText( string text )
    {
        if( MapExtensions.TryParseHeading( text, out var heading ) )
        {
            Heading = heading;
            return true;
        }

        _logger?.LogWarning( "Could not parse '{text}' into heading", text );
        return false;
    }
}
