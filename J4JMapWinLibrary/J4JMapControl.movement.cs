using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.J4JMapLibrary;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Input;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    private void OnPointerPressed( object sender, PointerRoutedEventArgs e )
    {
        CapturePointer( e.Pointer );
        _lastDragPoint = e.GetCurrentPoint( this );
    }

    private void OnPointerMoved( object sender, PointerRoutedEventArgs e )
    {
        if( PointerCaptures?.Any( p => p.PointerId == e.Pointer.PointerId ) ?? false )
            OnMapDragged( e.GetIntermediatePoints( this ) );
    }

    private void OnMapDragged( IList<PointerPoint> points )
    {
        // shouldn't be necessary, but...
        if( _lastDragPoint == null || MapRegion == null )
            return;

        foreach( var point in points )
        {
            _throttleMoves.Throttle( 5,
                                     _ =>
                                     {
                                         if( _lastDragPoint == null )
                                             return;

                                         var xDelta = point.Position.X - _lastDragPoint.Position.X;
                                         var yDelta = point.Position.Y - _lastDragPoint.Position.Y;

                                         _lastDragPoint = point;

                                         if( Math.Abs( xDelta ) == 0 && Math.Abs( yDelta ) == 0 )
                                             return;

                                         _logger.Warning( "Pointer moved {0}, {1}", xDelta, yDelta );

                                         MapRegion.Offset( (float) xDelta, (float) yDelta );
                                         MapRegion.Build();

                                         InvalidateMeasure();
                                     } );
        }
    }

    private void OnPointerReleased( object sender, PointerRoutedEventArgs e )
    {
        ReleasePointerCapture( e.Pointer );
        _lastDragPoint = null;
    }
}
