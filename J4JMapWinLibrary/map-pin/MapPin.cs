// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Numerics;
using Windows.Foundation;
using J4JSoftware.DeusEx;
using J4JSoftware.J4JMapLibrary;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Serilog;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class MapPin : Control
{
    private readonly ILogger _logger;

    private Path? _pinPath;

    public MapPin()
    {
        DefaultStyleKey = typeof( MapPin );

        _logger = J4JDeusEx.GetLogger() ?? throw new NullReferenceException( "Could not obtain ILogger instance" );
        _logger.ForContext( GetType() );
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _pinPath = FindUIElement<Path>( "ForegroundPath" );

        InitializePin();
    }

    private T? FindUIElement<T>( string name, Action<T>? postProcessor = null )
        where T : UIElement
    {
        var retVal = GetTemplateChild( name ) as T;
        if( retVal == null )
            _logger.Error( "Couldn't find {0}", name );
        else postProcessor?.Invoke( retVal );

        return retVal;
    }

    private void InitializePin()
    {
        if( _pinPath == null )
            return;

        var pathFigure = new PathFigure { IsClosed = true, IsFilled = true, StartPoint = new Point( 0, ArcRadius ) };

        pathFigure.Segments.Add( new ArcSegment
        {
            IsLargeArc = false,
            Point = new Point( 2 * ArcRadius, ArcRadius ),
            SweepDirection = SweepDirection.Clockwise,
            Size = new Size( ArcRadius, ArcRadius )
        } );

        pathFigure.Segments.Add( new LineSegment { Point = new Point( ArcRadius, ArcRadius + TailLength ) } );

        var geometry = new PathGeometry();
        geometry.Figures.Add( pathFigure );

        _pinPath.Data = geometry;
        _pinPath.Width = 2 * ArcRadius;
        _pinPath.Height = ArcRadius + TailLength;
    }

    protected override Size MeasureOverride( Size availableSize ) => new( 2 * ArcRadius, ArcRadius + TailLength );

    protected override Size ArrangeOverride( Size finalSize )
    {
        finalSize = base.ArrangeOverride( finalSize );

        // find the J4JMapControl
        J4JMapControl? mapControl = null;
        DependencyObject parent = this;

        while( mapControl == null )
        {
            parent = VisualTreeHelper.GetParent( parent );
            if( parent == null )
                break;

            mapControl = parent as J4JMapControl;
        }

        var region = mapControl?.MapRegion;
        if( region == null )
            return finalSize;

        if( !Location.TryParseCenter( this, out var latitude, out var longitude ) )
            return finalSize;

        var mapPoint = new MapPoint( region );
        mapPoint.SetLatLong( latitude, longitude );

        var upperLeft = region.UpperLeft.GetUpperLeftCartesian();

        var offset = region.ViewpointOffset;
        offset.X += mapPoint.X - upperLeft.X;
        offset.Y += mapPoint.Y - upperLeft.Y;

        if( region.Rotation % 360 != 0 )
        {
            var centerPoint = new Vector3( region.RequestedWidth / 2, region.RequestedHeight / 2, 0 );

            var transform = Matrix4x4.CreateRotationZ( region.Rotation * MapConstants.RadiansPerDegree, centerPoint );
            offset = Vector3.Transform( offset, transform );
        }

        // get the relative horizontal/vertical offsets for the UIElement
        offset.X += (float) ( HorizontalAlignment switch
        {
            HorizontalAlignment.Left => -finalSize.Width,
            HorizontalAlignment.Right => 0,
            _ => -finalSize.Width / 2
        } );

        offset.Y += (float) ( VerticalAlignment switch
        {
            VerticalAlignment.Top => 0,
            VerticalAlignment.Bottom => -finalSize.Height,
            _ => -finalSize.Height / 2
        } );

        // get the specific/custom offset
        Location.TryParseOffset( this, out var customOffset );

        offset.X += (float) customOffset.X;
        offset.Y += (float) customOffset.Y;

        //var xOffset = mapPoint.X - upperLeft.X + mapPointOffset.X + offset.X;
        //var yOffset = mapPoint.Y - upperLeft.Y + mapPointOffset.Y + offset.Y;

        Canvas.SetLeft( this, offset.X );
        Canvas.SetTop( this, offset.Y );

        return finalSize;
    }
}
