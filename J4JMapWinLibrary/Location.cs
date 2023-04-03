using Windows.Foundation;
using ABI.Microsoft.UI.Xaml.Controls;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.J4JMapLibrary.MapRegion;
using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public class Location : DependencyObject
{
    public static DependencyProperty CenterProperty =
        DependencyProperty.RegisterAttached( "Center",
                                             typeof( string ),
                                             typeof( Location ),
                                             new PropertyMetadata( null ) );

    public static string GetCenter(UIElement element) =>
        (string) element.GetValue(CenterProperty);

    public static void SetCenter( UIElement element, string value ) =>
        element.SetValue( CenterProperty, value );

    public static bool TryParseCenter( UIElement element, out float latitude, out float longitude )
    {
        latitude = 0;
        longitude = 0;

        var center = GetCenter( element );

        return !string.IsNullOrEmpty( center ) && Extensions.TryParseToLatLong( center, out latitude, out longitude );
    }

    public static DependencyProperty OffsetProperty =
        DependencyProperty.RegisterAttached("Offset",
                                            typeof(string),
                                            typeof(Location),
                                            new PropertyMetadata("0,0"));

    public static string GetOffset(UIElement element) =>
        (string)element.GetValue(OffsetProperty);

    public static void SetOffset(UIElement element, string value) =>
        element.SetValue(OffsetProperty, value);

    public static bool TryParseOffset(UIElement element, out Point offset )
    {
        offset = new Point();

        var offsetText = GetOffset(element);

        if( !Extensions.TryParseToPoint( offsetText, out var temp ) )
            return false;

        offset = temp!.Value;
        return true;
    }

    public static bool InRegion( FrameworkElement element, MapRegion region, out float xOffset, out float yOffset )
    {
        xOffset = 0;
        yOffset = 0;

        if( !TryParseCenter( element, out var latitude, out var longitude ) )
            return false;

        if( latitude < -MapConstants.Wgs84MaxLatitude || latitude > MapConstants.Wgs84MaxLatitude)
            return false;

        var mapPoint = new MapPoint( region );
        mapPoint.SetLatLong( latitude, longitude );

        var vpOffset = region.ViewpointOffset;
        var upperLeft = region.UpperLeft.GetUpperLeftCartesian();

        // get the relative horizontal/vertical offsets for the UIElement
        var hOffset = (float) ( element.HorizontalAlignment switch
        {
            HorizontalAlignment.Left => element.ActualWidth,
            HorizontalAlignment.Right => 0,
            _ => element.ActualWidth / 2
        } );

        var vOffset = (float) ( element.VerticalAlignment switch
        {
            VerticalAlignment.Top => 0,
            VerticalAlignment.Bottom => element.ActualHeight,
            _ => element.ActualHeight / 2
        } );

        xOffset = mapPoint.X - upperLeft.X + vpOffset.X - hOffset;
        yOffset = mapPoint.Y - upperLeft.Y + vpOffset.Y - vOffset;

        return mapPoint.X >= upperLeft.X
         && mapPoint.X < upperLeft.X + region.RequestedWidth + region.Projection.TileHeightWidth
         && mapPoint.Y >= upperLeft.Y
         && mapPoint.Y < upperLeft.Y + region.RequestedHeight + region.Projection.TileHeightWidth;
    }
}
