// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// 
// This file is part of ConsoleUtilities.
//
// ConsoleUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// ConsoleUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with ConsoleUtilities. If not, see <https://www.gnu.org/licenses/>.

namespace J4JSoftware.J4JMapLibrary;

public class Cartesian
{
    public EventHandler? Changed;

    public Cartesian(
        ITiledScale scale
    )
    {
        XRange = scale.XRange;
        YRange = scale.YRange;
    }

    public Cartesian()
    {
        XRange = new MinMax<int>( int.MinValue, int.MaxValue );
        YRange = new MinMax<int>( int.MinValue, int.MaxValue );
    }

    public MinMax<int> XRange { get; internal set; }
    public int X { get; private set; }

    public MinMax<int> YRange { get; internal set; }
    public int Y { get; private set; }

    public void SetCartesian( int? x, int? y )
    {
        if( x == null && y == null )
            return;

        if( x.HasValue )
            X = XRange.ConformValueToRange( x.Value, "X" );

        if( y.HasValue )
            Y = YRange.ConformValueToRange( y.Value, "Y" );

        Changed?.Invoke( this, EventArgs.Empty );
    }

    public void SetCartesian( Cartesian cartesian )
    {
        X = XRange.ConformValueToRange( cartesian.X, "X" );
        Y = YRange.ConformValueToRange( cartesian.Y, "Y" );

        Changed?.Invoke( this, EventArgs.Empty );
    }
}
