using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using System.Diagnostics;

namespace J4JSoftware.J4JMapWinLibrary;

public enum MovementType
{
    Translation,
    Rotation,
    Undefined
}

public record Movement( MovementType Type, Point Position );

public class MovementProcessor
{
    public event EventHandler<Movement>? Moved;
    public event EventHandler<MovementType>? MovementsEnded;

    private record KeyedPoint( MovementType MovementType, PointerPoint Point );

    private readonly DispatcherTimer _timer = new();
    private readonly Queue<KeyedPoint> _points = new();

    private PointerPoint? _lastProcessed;
    private ulong _lastProcessedTimestamp = ulong.MinValue;

    public MovementProcessor()
    {
        _timer.Interval = TimeSpan.FromMilliseconds( 125 );
        _timer.Tick += TimerOnTick;
    }

    public bool Enabled { get; set; }
    public MovementType MovementType { get; private set; } = MovementType.Undefined;

    public void AddPoints( IList<PointerPoint> points, bool controlPressed )
    {
        if( !Enabled )
            return;

        var lastTs = _points.LastOrDefault()?.Point.Timestamp ?? 0UL;

        for( var idx = 0; idx < points.Count; idx++ )
        {
            var curPoint = points[idx];

            if( idx == 0 && _lastProcessed == null )
            {
                _lastProcessed = curPoint;
                _lastProcessedTimestamp = _lastProcessed.Timestamp;

                lastTs = curPoint.Timestamp;
            }
            else
            {
                // skip points whose timestamps are too close together in time
                if( curPoint.Timestamp - lastTs < 100000 )
                    continue;

                lastTs = curPoint.Timestamp;

                _points.Enqueue( new KeyedPoint( controlPressed ? MovementType.Rotation : MovementType.Translation,
                                                 curPoint ) );

                if( !_timer.IsEnabled )
                    _timer.Start();
            }
        }
    }

    private void TimerOnTick( object? sender, object e )
    {
        if( !Enabled )
            _points.Clear();

        if ( _points.Any() )
            ProcessPoints();
        else EndProcessing();
    }

    private void ProcessPoints()
    {
        // find the first PointerPoint that is Interval microseconds
        // later than the last one processed, or the last PointerPoint
        // if none fits that filter
        KeyedPoint? toProcess = null;

        while (toProcess == null)
        {
            var dequeued = _points.Dequeue();

            if (!_points.Any() || dequeued.Point.Timestamp - _lastProcessedTimestamp >= _timer.Interval.TotalMicroseconds)
                toProcess = dequeued;
        }

        _lastProcessedTimestamp = toProcess.Point.Timestamp;

        if (toProcess.MovementType != MovementType)
        {
            if (MovementType != MovementType.Undefined)
                MovementsEnded?.Invoke(this, MovementType);

            MovementType = toProcess.MovementType;
        }

        switch( MovementType )
        {
            case MovementType.Rotation:
            case MovementType.Translation:
                Moved?.Invoke(this, new Movement(MovementType, toProcess.Point.Position));
                break;

            case MovementType.Undefined:
            default:
                // no op
                break;
        }
    }

    private void EndProcessing()
    {
        _timer.Stop();

        switch (MovementType)
        {
            case MovementType.Rotation:
            case MovementType.Translation:
                MovementsEnded?.Invoke(this, MovementType);
                break;

            case MovementType.Undefined:
            default:
                // no op
                break;
        }

        _lastProcessed = null;
        MovementType = MovementType.Undefined;
    }
}
