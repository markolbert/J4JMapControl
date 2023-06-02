﻿#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// PointOfInterest.cs
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

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;

namespace WinAppTest;

public class PointOfInterest : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _location = "0E, 0N";
    private string _text = string.Empty;
    private SolidColorBrush _brush = new( Colors.Black );

    public string Location
    {
        get => _location;
        set => SetField( ref _location, value );
    }

    public string Text
    {
        get => _text;
        set => SetField( ref _text, value );
    }

    public SolidColorBrush Brush
    {
        get => _brush;
        set => SetField( ref _brush, value );
    }

    protected virtual void OnPropertyChanged( [ CallerMemberName ] string? propertyName = null )
    {
        PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
    }

    protected bool SetField<T>( ref T field, T value, [ CallerMemberName ] string? propertyName = null )
    {
        if( EqualityComparer<T>.Default.Equals( field, value ) )
            return false;

        field = value;
        OnPropertyChanged( propertyName );
        return true;
    }
}
