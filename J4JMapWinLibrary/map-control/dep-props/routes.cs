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

using System;
using System.Collections.ObjectModel;
using J4JSoftware.WindowsUtilities;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    private readonly ThrottleDispatcher _throttleRouteChanges = new();

    private Canvas? _routesCanvas;

    public static readonly DependencyProperty MapRoutesProperty =
        DependencyProperty.Register( nameof( MapRoutes ),
                                     typeof( ObservableCollection<MapRoute> ),
                                     typeof( J4JMapControl ),
                                     new PropertyMetadata( new ObservableCollection<MapRoute>() ) );

    public ObservableCollection<MapRoute> MapRoutes
    {
        get => (ObservableCollection<MapRoute>) GetValue( MapRoutesProperty );
        set
        {
            foreach( var route in MapRoutes )
            {
                route.Changed -= RouteChanged;
            }

            SetValue( MapRoutesProperty, value );

            foreach( var route in MapRoutes )
            {
                route.Changed += RouteChanged;
            }
        }
    }

    private void RouteChanged( object? sender, EventArgs e )
    {
        _throttleRouteChanges.Throttle( UpdateEventInterval, _ => IncludeRoutes() );
    }
}
