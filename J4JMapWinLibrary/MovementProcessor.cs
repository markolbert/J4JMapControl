using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.J4JMapLibrary.MapRegion;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Serilog;

namespace J4JSoftware.J4JMapWinLibrary;

public class MovementProcessor
{
    private readonly DispatcherTimer _timer = new();
    private readonly MapRegion _region;
    private readonly Queue<PointerPoint> _points = new();
    private readonly ILogger _logger;

    private PointerPoint? _lastProcessed;
    private ulong _lastProcessedTimestamp = ulong.MinValue;

    public MovementProcessor(
        MapRegion region,
        ILogger logger
    )
    {
        _region = region;
        _timer.Interval = TimeSpan.FromMilliseconds( 125 );
        _timer.Tick += TimerOnTick;

        _logger = logger;
        _logger.ForContext<MovementProcessor>();
    }

    public void AddPoints( IList<PointerPoint> points )
    {
        var lastTs = _points.LastOrDefault()?.Timestamp ?? 0UL;

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

                _points.Enqueue( curPoint );

                if (!_timer.IsEnabled)
                    _timer.Start();
            }
        }
    }

    private void TimerOnTick( object? sender, object e )
    {
        if( !_points.Any() )
        {
            _timer.Stop();
            _lastProcessed = null;
            return;
        }

        // find the first PointerPoint that is Interval microseconds
        // later than the last one processed, or the last PointerPoint
        // if none fits that filter
        PointerPoint? toProcess = null;

        while( toProcess == null )
        {
            var dequeued = _points.Dequeue();

            if( !_points.Any() || dequeued.Timestamp - _lastProcessedTimestamp >= _timer.Interval.TotalMicroseconds )
                toProcess = dequeued;
        }

        var xDelta = _lastProcessed!.Position.X - toProcess.Position.X;
        var yDelta = _lastProcessed.Position.Y - toProcess.Position.Y;

        if( Math.Abs( xDelta ) < 5 && Math.Abs( yDelta ) < 5 )
            return;

        _lastProcessed = toProcess;

        _logger.Verbose( "Moving map by ({0:n1}, {1:n1}), timestamp {2}", xDelta, yDelta, toProcess.Timestamp );

        _region.Offset( (float) xDelta, (float) yDelta );
        _region.Build();
    }
}
