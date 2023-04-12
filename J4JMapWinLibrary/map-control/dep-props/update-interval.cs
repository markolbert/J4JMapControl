#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// update-interval.cs
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

using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    public DependencyProperty UpdateEventIntervalProperty = DependencyProperty.Register( nameof( UpdateEventInterval ),
        typeof( int ),
        typeof( J4JMapControl ),
        new PropertyMetadata( DefaultUpdateEventInterval, OnUpdateIntervalChanged ) );

    public int UpdateEventInterval
    {
        get => (int) GetValue( UpdateEventIntervalProperty );
        set => SetValue( UpdateEventIntervalProperty, value );
    }

    private static void OnUpdateIntervalChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
        if( d is not J4JMapControl mapControl )
            return;

        if( e.NewValue is not int value )
            return;

        if( value < 0 )
        {
            mapControl._logger?.LogWarning( "Tried to set UpdateEventInterval < 0, defaulting to {0}",
                                            DefaultUpdateEventInterval );
            value = DefaultUpdateEventInterval;
        }

        mapControl.UpdateEventInterval = value;
    }
}
