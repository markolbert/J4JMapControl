// Copyright (c) 2021, 2022 Mark A. Olbert 
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

namespace J4JMapLibrary;

public class TiledScale : ProjectionScale, ITiledScale
{
    private MinMax<int>? _xRange;
    private MinMax<int>? _yRange;

    public TiledScale(
        IMapServer mapServer
    )
        : base( mapServer )
    {
    }

    private TiledScale( TiledScale toCopy )
        : base( toCopy )
    {
    }

    public MinMax<int> XRange
    {
        get
        {
            if( _xRange != null )
                return _xRange;

            var pow2 = InternalExtensions.Pow( 2, Scale );
            _xRange = new MinMax<int>( 0, MapServer.TileHeightWidth * pow2 - 1 );

            return _xRange;
        }
    }

    public MinMax<int> YRange
    {
        get
        {
            if( _yRange != null )
                return _yRange;

            var pow2 = InternalExtensions.Pow( 2, Scale );
            _yRange = new MinMax<int>( 0, MapServer.TileHeightWidth * pow2 - 1 );

            return _yRange;
        }
    }

    protected override void UpdateScaleRelated()
    {
        base.UpdateScaleRelated();

        _xRange = null;
        _yRange = null;
    }

    public bool Equals( TiledScale? other )
    {
        if( ReferenceEquals( null, other ) ) return false;
        if( ReferenceEquals( this, other ) ) return true;

        return base.Equals( other ) && XRange.Equals( other.XRange ) && YRange.Equals( other.YRange );
    }

    public static TiledScale Copy( TiledScale toCopy ) => new( toCopy );

    public override bool Equals( object? obj )
    {
        if( ReferenceEquals( null, obj ) ) return false;
        if( ReferenceEquals( this, obj ) ) return true;

        return obj.GetType() == GetType() && Equals( (TiledScale) obj );
    }

    public override int GetHashCode() => HashCode.Combine( base.GetHashCode(), XRange, YRange );

    public static bool operator==( TiledScale? left, TiledScale? right ) => Equals( left, right );

    public static bool operator!=( TiledScale? left, TiledScale? right ) => !Equals( left, right );
}
