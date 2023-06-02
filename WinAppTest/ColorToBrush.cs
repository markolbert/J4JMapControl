#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// ColorToBrush.cs
//
// This file is part of JumpForJoy Software's WinAppTest.
// 
// WinAppTest is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// WinAppTest is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with WinAppTest. If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using Windows.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace WinAppTest;

internal class ColorToBrush : IValueConverter
{
    public object Convert( object value, Type targetType, object parameter, string language )
    {
        if( value is not Color color || targetType != typeof( Brush ) )
        {
            throw new ArgumentException(
                $"Expected to convert {typeof( Color )} to {typeof( Brush )}, but got {value.GetType()}, {targetType} instead" );
        }

        return new SolidColorBrush( color );
    }

    public object ConvertBack( object value, Type targetType, object parameter, string language ) =>
        throw new NotImplementedException();
}
