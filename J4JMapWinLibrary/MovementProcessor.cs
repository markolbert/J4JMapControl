using System;
using System.Collections.Generic;
using System.Linq;
using Windows.System;
using Windows.UI.Core;
using J4JSoftware.J4JMapLibrary;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Serilog;

namespace J4JSoftware.J4JMapWinLibrary;

public class MovementProcessor
{
    public event EventHandler? RotationsStarted;
    public event EventHandler? RotationsEnded;
    public event EventHandler<RotationInfo>? Rotated;
    
    private record KeyedPoint( bool ControlPressed, PointerPoint Point );

    private readonly DispatcherTimer _timer = new();
    private readonly J4JMapControl _mapControl;
    private readonly Queue<KeyedPoint> _points = new();
    private readonly ILogger _logger;

    private PointerPoint? _lastProcessed;
    private float? _prevAngle;
    private PointerPoint? _firstRotationTip;
    private float? _firstRotationAngle;
    private ulong _lastProcessedTimestamp = ulong.MinValue;
    private bool _handlingRotations;

    public MovementProcessor(
        J4JMapControl mapControl,
        ILogger logger
    )
    {
        _mapControl = mapControl;

        _timer.Interval = TimeSpan.FromMilliseconds( 125 );
        _timer.Tick += TimerOnTick;

        _logger = logger;
        _logger.ForContext<MovementProcessor>();
    }

    public bool Enabled { get; set; }

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

                _points.Enqueue( new KeyedPoint( controlPressed, curPoint ) );

                if( !_timer.IsEnabled )
                    _timer.Start();
            }
        }
    }

    private void TimerOnTick( object? sender, object e )
    {
        if( _mapControl.MapRegion == null )
            return;

        if( !Enabled )
            _points.Clear();

        if ( !_points.Any() )
        {
            EndProcessing();
            return;
        }

        // find the first PointerPoint that is Interval microseconds
        // later than the last one processed, or the last PointerPoint
        // if none fits that filter
        KeyedPoint? toProcess = null;

        while( toProcess == null )
        {
            var dequeued = _points.Dequeue();

            if( !_points.Any() || dequeued.Point.Timestamp - _lastProcessedTimestamp >= _timer.Interval.TotalMicroseconds )
                toProcess = dequeued;
        }

        if( toProcess.ControlPressed )
            ProcessRotation(toProcess);
        else ProcessTranslation(toProcess);
    }

    private void EndProcessing()
    {
        _timer.Stop();
        _lastProcessed = null;

        _prevAngle = null;
        _firstRotationTip = null;
        _firstRotationAngle = null;

        if (!_handlingRotations)
            return;

        _handlingRotations = false;
        RotationsEnded?.Invoke(this, EventArgs.Empty);
    }

    private float CalculateAngle( KeyedPoint toProcess ) =>
        (float) Math.Atan2( _mapControl.ActualHeight / 2 - toProcess.Point.Position.Y,
                            toProcess.Point.Position.X - _mapControl.ActualWidth / 2 )
      * MapConstants.DegreesPerRadian;

    private void ProcessTranslation( KeyedPoint toProcess )
    {
        var xDelta = _lastProcessed!.Position.X - toProcess.Point.Position.X;
        var yDelta = _lastProcessed.Position.Y - toProcess.Point.Position.Y;

        if (Math.Abs(xDelta) < 5 && Math.Abs(yDelta) < 5)
            return;

        _lastProcessed = toProcess.Point;

        _logger.Verbose( "Moving map by ({0:n1}, {1:n1}), timestamp {2}", xDelta, yDelta, toProcess.Point.Timestamp );

        _mapControl.MapRegion!.Offset((float)xDelta, (float)yDelta);
        _mapControl.MapRegion!.Build();
    }

    private void ProcessRotation( KeyedPoint toProcess )
    {
        if( !_handlingRotations )
            StartRotationHinting( toProcess );

        if( _prevAngle == null )
        {
            _prevAngle = CalculateAngle( toProcess );

            _logger.Verbose( "Initial angle is {0:n1} degrees, timestamp {1}",
                             _prevAngle.Value,
                             toProcess.Point.Timestamp );
        }

        var newAngle = CalculateAngle( toProcess );

        // positive values of rotation are COUNTER CLOCKWISE
        // negative values are CLOCKWISE
        var rotation = newAngle - _prevAngle.Value;
        if( Math.Abs( rotation ) < 2 )
            return;

        _logger.Verbose( "New angle is {0:n1}, previous angle is {1:n1}, rotating map by {2:n1} degrees, timestamp {3}",newAngle, _prevAngle, rotation, toProcess.Point.Timestamp );

        _mapControl.MapRegion!.Heading( _mapControl.MapRegion!.Heading + rotation );
        _mapControl.MapRegion!.Build();

        Rotated?.Invoke( this,
                         new RotationInfo( _firstRotationTip!, toProcess.Point, newAngle - _firstRotationAngle!.Value ) );

        _prevAngle = newAngle;
    }

    private void StartRotationHinting( KeyedPoint toProcess )
    {
        _handlingRotations = true;
        _prevAngle = null;
        _firstRotationTip = toProcess.Point;
        _firstRotationAngle = CalculateAngle(toProcess);

        RotationsStarted?.Invoke(this, EventArgs.Empty);
    }
}
