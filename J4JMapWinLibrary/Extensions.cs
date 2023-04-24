#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// Extensions.cs
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
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using J4JSoftware.J4JMapLibrary;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace J4JSoftware.J4JMapWinLibrary;

public static class Extensions
{
    public static bool TryParseToPoint( string? text, out Point? point )
    {
        point = null;

        if( string.IsNullOrEmpty( text ) )
            return false;

        var parts = text.Split( ',', ' ' );
        if( parts.Length != 2 )
            return false;

        if( !float.TryParse( parts[ 0 ], out var xOffset ) )
            return false;

        if( !float.TryParse( parts[ 1 ], out var yOffset ) )
            return false;

        point = new Point( xOffset, yOffset );
        return true;
    }

    public static double AngleFromCenter( this FrameworkElement control, Point point ) =>
        AngleBetweenPoints( new Point( control.ActualWidth / 2, control.ActualHeight / 2 ), point );

    public static double AngleBetweenPoints( Point origin, Point point ) =>
        Math.Atan2( origin.Y - point.Y, point.X - origin.X )
      * MapConstants.DegreesPerRadian;
}
