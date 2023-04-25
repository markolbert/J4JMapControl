#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// annotations.cs
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

using J4JSoftware.WindowsUtilities;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    private readonly PointsOfInterestPositions _pointsOfInterest;
    private readonly ThrottleDispatcher _throttlePoIChanges = new();

    private Canvas? _poiCanvas;

    public static readonly DependencyProperty PoILatitudeProperty =
        DependencyProperty.Register( nameof( PoILatitude ),
                                     typeof( string ),
                                     typeof( J4JMapControl ),
                                     new PropertyMetadata( null ) );

    public string? PoILatitude
    {
        get => (string?) GetValue(PoILatitudeProperty);

        set
        {
            SetValue( PoILatitudeProperty, value );
            _pointsOfInterest.LatitudeProperty = value;
        }
    }

    public static readonly DependencyProperty PoILongitudeProperty =
        DependencyProperty.Register(nameof(PoILongitude),
                                    typeof(string),
                                    typeof(J4JMapControl),
                                    new PropertyMetadata(null));

    public string? PoILongitude
    {
        get => (string?)GetValue(PoILongitudeProperty);

        set
        {
            SetValue( PoILongitudeProperty, value );
            _pointsOfInterest.LongitudeProperty = value;
        }
    }

    public static readonly DependencyProperty PoILatLongProperty =
        DependencyProperty.Register(nameof(PoILatLong),
                                    typeof(string),
                                    typeof(J4JMapControl),
                                    new PropertyMetadata(null));

    public string? PoILatLong
    {
        get => (string?)GetValue(PoILatLongProperty);

        set
        {
            SetValue( PoILatLongProperty, value );
            _pointsOfInterest.LatLongProperty = value;
        }
    }

    public static readonly DependencyProperty PoIDataSourceProperty =
        DependencyProperty.Register(nameof(PoIDataSource),
                                    typeof(object),
                                    typeof(J4JMapControl),
                                    new PropertyMetadata(null));

    public object? PoIDataSource
    {
        get => GetValue(PoIDataSourceProperty);

        set
        {
            SetValue( PoIDataSourceProperty, value );
            _pointsOfInterest.Source = value;
        }
    }

    public DataTemplate? PointsOfInterestTemplate { get; set; }

    private void PointsOfInterestSourceUpdated(object? sender, EventArgs e)
    {
        _throttlePoIChanges.Throttle( UpdateEventInterval, _ => IncludePointsOfInterest() );
    }

}
