#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// minmax-scale.cs
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

using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    public DependencyProperty MinMapScaleProperty = DependencyProperty.Register( nameof( MinMapScale ),
        typeof( double ),
        typeof( J4JMapControl ),
        new PropertyMetadata( 0.0 ) );

    public double MinMapScale
    {
        get => (double) GetValue( MinMapScaleProperty );

        private set
        {
            if( value < _projection?.MinScale )
                value = _projection.MinScale;

            SetValue( MinMapScaleProperty, value );

            if( MapScale < value )
                MapScale = value;
        }
    }

    public DependencyProperty MaxMapScaleProperty = DependencyProperty.Register(nameof(MaxMapScale),
        typeof(double),
        typeof(J4JMapControl),
        new PropertyMetadata(20.0));

    public double MaxMapScale
    {
        get => (double) GetValue( MaxMapScaleProperty );

        private set
        {
            if( value > _projection?.MaxScale )
                value = _projection.MaxScale;

            SetValue( MaxMapScaleProperty, value );

            if( MapScale > value )
                MapScale = value;
        }
    }
}
