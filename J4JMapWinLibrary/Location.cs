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
    public static DependencyProperty CenterProperty =
        DependencyProperty.RegisterAttached( "Center",
                                             typeof( string ),
                                             typeof( Location ),
                                             new PropertyMetadata( null ) );

    public static DependencyProperty OffsetProperty =
        DependencyProperty.RegisterAttached( "Offset",
                                             typeof( string ),
                                             typeof( Location ),
                                             new PropertyMetadata( "0,0" ) );

    public static string GetCenter( UIElement element ) => (string) element.GetValue( CenterProperty );

    public static void SetCenter( UIElement element, string value ) => element.SetValue( CenterProperty, value );

    public static bool TryParseCenter( UIElement element, out float latitude, out float longitude )
    {
        latitude = 0;
        longitude = 0;

        var center = GetCenter( element );

        return !string.IsNullOrEmpty( center ) && Extensions.TryParseToLatLong( center, out latitude, out longitude );
    }

    public static string GetOffset( UIElement element ) => (string) element.GetValue( OffsetProperty );

    public static void SetOffset( UIElement element, string value ) => element.SetValue( OffsetProperty, value );

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
        if( !TryParseCenter( element, out var latitude, out var longitude ) )
            return false;

        if( latitude < -MapConstants.Wgs84MaxLatitude || latitude > MapConstants.Wgs84MaxLatitude )
            return false;

        var mapPoint = new MapPoint( region );
        mapPoint.SetLatLong( latitude, longitude );

        var upperLeft = region.UpperLeft.GetUpperLeftCartesian();

        return mapPoint.X >= upperLeft.X
         && mapPoint.X < upperLeft.X + region.RequestedWidth + region.Projection.TileHeightWidth
         && mapPoint.Y >= upperLeft.Y
         && mapPoint.Y < upperLeft.Y + region.RequestedHeight + region.Projection.TileHeightWidth;
    }
}
