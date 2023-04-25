#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// Location.cs
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

using Windows.Foundation;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.J4JMapLibrary.MapRegion;
using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public class Location : DependencyObject
{
    public static readonly DependencyProperty LatLongProperty =
        DependencyProperty.RegisterAttached( "LatLong",
                                             typeof( string ),
                                             typeof( Location ),
                                             new PropertyMetadata( null ) );

    public static string GetLatLong(UIElement element) => (string)element.GetValue(LatLongProperty);
    public static void SetLatLong(UIElement element, string value) => element.SetValue(LatLongProperty, value);

    public static readonly DependencyProperty LatitudeProperty =
        DependencyProperty.RegisterAttached( "Latitude",
                                             typeof( string ),
                                             typeof( Location ),
                                             new PropertyMetadata( null ) );

    public static string GetLatitude(UIElement element)=> (string) element.GetValue(LatitudeProperty);
    public static void SetLatitude(UIElement element, string value) => element.SetValue(LatitudeProperty, value);

    public static readonly DependencyProperty LongitudeProperty =
        DependencyProperty.RegisterAttached("Longitude",
                                            typeof(string),
                                            typeof(Location),
                                            new PropertyMetadata(null));

    public static string GetLongitude(UIElement element) => (string)element.GetValue(LongitudeProperty);
    public static void SetLongitude(UIElement element, string value) => element.SetValue(LongitudeProperty, value);

    public static bool TryParseLatLong(UIElement element, out float latitude, out float longitude)
    {
        // LatitudeProperty and LongitudeProperty, if valid, win out over any LatLongProperty value
        if( MapExtensions.TryParseToLatitude(GetLatitude(element), out latitude)
           && MapExtensions.TryParseToLongitude(GetLongitude(element), out longitude))
            return true;

        return MapExtensions.TryParseToLatLong( GetLatLong( element ), out latitude, out longitude );
    }

    public static readonly DependencyProperty OffsetProperty =
        DependencyProperty.RegisterAttached( "Offset",
                                             typeof( string ),
                                             typeof( Location ),
                                             new PropertyMetadata( "0,0" ) );

    public static string GetOffset(UIElement element) => (string)element.GetValue(OffsetProperty);

    public static void SetOffset(UIElement element, string value) => element.SetValue(OffsetProperty, value);

    public static bool TryParseOffset( UIElement element, out Point offset )
    {
        offset = new Point();

        var offsetText = GetOffset( element );

        if( !Extensions.TryParseToPoint( offsetText, out var temp ) )
            return false;

        offset = temp!.Value;
        return true;
    }

    public static bool InRegion( FrameworkElement element, MapRegion region )
    {
        if( !TryParseLatLong( element, out var latitude, out var longitude ) )
            return false;

        if( latitude < -MapConstants.Wgs84MaxLatitude || latitude > MapConstants.Wgs84MaxLatitude )
            return false;

        var mapPoint = new MapPoint( region );
        mapPoint.SetLatLong( latitude, longitude );

        var (upperLeftX, upperLeftY) = region.UpperLeft.GetUpperLeftCartesian();

        return mapPoint.X >= upperLeftX
         && mapPoint.X < upperLeftX + region.RequestedWidth + region.Projection.TileHeightWidth
         && mapPoint.Y >= upperLeftY
         && mapPoint.Y < upperLeftY + region.RequestedHeight + region.Projection.TileHeightWidth;
    }

    public static bool InRegion(IPlacedItem item, MapRegion region)
    {
        if (item.Latitude < -MapConstants.Wgs84MaxLatitude || item.Latitude > MapConstants.Wgs84MaxLatitude)
            return false;

        var mapPoint = new MapPoint(region);
        mapPoint.SetLatLong(item.Latitude, item.Longitude);

        var (upperLeftX, upperLeftY) = region.UpperLeft.GetUpperLeftCartesian();

        return mapPoint.X >= upperLeftX
         && mapPoint.X < upperLeftX + region.RequestedWidth + region.Projection.TileHeightWidth
         && mapPoint.Y >= upperLeftY
         && mapPoint.Y < upperLeftY + region.RequestedHeight + region.Projection.TileHeightWidth;
    }
}
