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

public class TiledBounds : IEquatable<TiledBounds>
{
    public TiledBounds(
        TileCoordinates upperLeft,
        TileCoordinates lowerRight
    )
    {
        UpperLeft = upperLeft;
        LowerRight = lowerRight;
    }

    public TileCoordinates UpperLeft { get; init; }
    public TileCoordinates LowerRight { get; init; }

    public bool Equals( TiledBounds? other )
    {
        if( ReferenceEquals( null, other ) ) return false;
        if( ReferenceEquals( this, other ) ) return true;

        return UpperLeft.Equals( other.UpperLeft ) && LowerRight.Equals( other.LowerRight );
    }

    public override bool Equals( object? obj )
    {
        if( ReferenceEquals( null, obj ) ) return false;
        if( ReferenceEquals( this, obj ) ) return true;

        return obj.GetType() == GetType() && Equals( (TiledBounds) obj );
    }

    public override int GetHashCode() => HashCode.Combine( UpperLeft, LowerRight );

    public static bool operator==( TiledBounds? left, TiledBounds? right ) => Equals( left, right );

    public static bool operator!=( TiledBounds? left, TiledBounds? right ) => !Equals( left, right );
}
