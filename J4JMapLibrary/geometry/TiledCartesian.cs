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

public class TiledCartesian
{
    public EventHandler? Changed;

    private readonly MinMax<int> _xRange;
    private readonly MinMax<int> _yRange;

    public TiledCartesian(
        ITiledProjection projection
    )
    {
        if( projection.TiledScale == null )
            throw new NullReferenceException( $"ITiledProjection is not initialized" );

        _xRange = projection.TiledScale.XRange;
        _yRange = projection.TiledScale.YRange;
    }

    public TiledCartesian()
    {
        _xRange = new MinMax<int>( int.MinValue, int.MaxValue );
        _yRange = new MinMax<int>( int.MinValue, int.MaxValue );
    }

    public int X { get; private set; }

    public int Y { get; private set; }

    public void SetCartesian( int? x, int? y )
    {
        if( x == null && y == null )
            return;

        if( x.HasValue )
            X = _xRange.ConformValueToRange( x.Value, "X" );

        if( y.HasValue )
            Y = _yRange.ConformValueToRange( y.Value, "Y" );

        Changed?.Invoke( this, EventArgs.Empty );
    }

    public void SetCartesian( TiledCartesian tiledCartesian )
    {
        X = _xRange.ConformValueToRange( tiledCartesian.X, "X" );
        Y = _yRange.ConformValueToRange( tiledCartesian.Y, "Y" );

        Changed?.Invoke( this, EventArgs.Empty );
    }
}
