using System;
using Windows.Foundation;
using J4JSoftware.VisualUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using J4JSoftware.J4JMapLibrary;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Numerics;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    private T? FindUiElement<T>( string name, Action<T>? postProcessor = null )
        where T : UIElement
    {
        var retVal = GetTemplateChild( name ) as T;
        if( retVal == null )
            _logger?.LogError( "Couldn't find {name}", name );
        else postProcessor?.Invoke( retVal );

        return retVal;
    }

    private Point ViewPointToRegionPoint(Point point)
    {
        if (RegionView == null || ShrinkStyle == ShrinkStyle.None)
            return point;

        return Zoom == null
            ? point
            : new Point(point.X * Zoom.Value, point.Y * Zoom.Value);
    }

    private Point RegionPointToViewPoint(Point point)
    {
        if (RegionView == null || ShrinkStyle == ShrinkStyle.None)
            return point;

        return Zoom == null
            ? point
            : new Point(point.X / Zoom.Value, point.Y / Zoom.Value);
    }

    private bool InRegion(FrameworkElement element)
    {
        if (_projection == null || MapRectangle == null)
            return false;

        if (!Location.TryParseLatLong(element, out var latitude, out var longitude))
            return false;

        if (latitude < -MapConstants.Wgs84MaxLatitude || latitude > MapConstants.Wgs84MaxLatitude)
            return false;

        var mapPoint = new MapPoint(_projection, (int)MapScale);
        mapPoint.SetLatLong(latitude, longitude);

        return mapPoint.X >= MapUpperLeft.X
         && mapPoint.X < MapUpperLeft.X + ActualWidth + _projection.TileHeightWidth
         && mapPoint.Y >= MapUpperLeft.Y
         && mapPoint.Y < MapUpperLeft.Y + ActualHeight + _projection.TileHeightWidth;
    }

    private bool InRegion(IPlacedItem item)
    {
        if (_projection == null || MapRectangle == null)
            return false;

        if (item.Latitude < -MapConstants.Wgs84MaxLatitude || item.Latitude > MapConstants.Wgs84MaxLatitude)
            return false;

        var mapPoint = new MapPoint(_projection, (int)MapScale);
        mapPoint.SetLatLong(item.Latitude, item.Longitude);

        return mapPoint.X >= MapUpperLeft.X
         && mapPoint.X < MapUpperLeft.X + ActualWidth + _projection.TileHeightWidth
         && mapPoint.Y >= MapUpperLeft.Y
         && mapPoint.Y < MapUpperLeft.Y + ActualHeight + _projection.TileHeightWidth;
    }

    private void DefineColumns()
    {
        if (_loadedRegion == null)
            return;

        var cellWidth = _projection switch
        {
            StaticProjection => GridLength.Auto,
            ITiledProjection => new GridLength(_projection.TileHeightWidth),
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {typeof(IProjection)} value '{_projection?.GetType()}'")
        };

        _mapGrid!.ColumnDefinitions.Clear();

        var tilesWide = _loadedRegion.LastColumn - _loadedRegion.FirstColumn + 1;

        for (var column = 0; column < tilesWide; column++)
        {
            _mapGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = cellWidth });
        }
    }

    private void DefineRows()
    {
        if (_loadedRegion == null)
            return;

        var cellHeight = _projection switch
        {
            StaticProjection => GridLength.Auto,
            ITiledProjection => new GridLength(_projection.TileHeightWidth),
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {typeof(IProjection)}  value ' {_projection?.GetType()}'")
        };

        _mapGrid!.RowDefinitions.Clear();

        var tilesHigh = _loadedRegion.LastRow - _loadedRegion.FirstRow + 1;

        for (var row = 0; row < tilesHigh; row++)
        {
            _mapGrid.RowDefinitions.Add(new RowDefinition { Height = cellHeight });
        }
    }

    private static bool SizeIsValid(Size toCheck) =>
        toCheck.Width != 0
     && toCheck.Height != 0
     && !double.IsPositiveInfinity(toCheck.Height)
     && !double.IsPositiveInfinity(toCheck.Width);

    private Vector3 GetDisplayPosition(float latitude, float longitude)
    {
        if (_projection == null || _loadedRegion == null || MapCenterPoint == null)
            return Vector3.Zero;

        var mapPoint = new MapPoint(_projection, (int)MapScale);
        mapPoint.SetLatLong(latitude, longitude);

        var position = _loadedRegion.Offset;
        position.X += mapPoint.X - MapUpperLeft.X;
        position.Y += mapPoint.Y - MapUpperLeft.Y;

        if (MapRotation % 360 == 0)
            return position;

        var transform =
            Matrix4x4.CreateRotationZ(MapRotation * MapConstants.RadiansPerDegree,
                                      new Vector3(MapCenterPoint.X, MapCenterPoint.Y, 0));

        position = Vector3.Transform(position, transform);

        return position;
    }

}
