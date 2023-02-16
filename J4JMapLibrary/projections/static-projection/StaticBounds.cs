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

namespace J4JSoftware.J4JMapLibrary;

public class StaticBounds : IEquatable<StaticBounds>
{
    #region IEquatable...

    public bool Equals( StaticBounds? other )
    {
        if( ReferenceEquals( null, other ) )
            return false;
        if( ReferenceEquals( this, other ) )
            return true;

        return CenterLatitude.Equals( other.CenterLatitude )
         && CenterLongitude.Equals( other.CenterLongitude )
         && Height.Equals( other.Height )
         && Width.Equals( other.Width );
    }

    public override bool Equals( object? obj )
    {
        if( ReferenceEquals( null, obj ) )
            return false;
        if( ReferenceEquals( this, obj ) )
            return true;

        return obj.GetType() == GetType() && Equals( (StaticBounds) obj );
    }

    public override int GetHashCode() => HashCode.Combine( CenterLatitude, CenterLongitude, Height, Width );

    public static bool operator==( StaticBounds? left, StaticBounds? right ) => Equals( left, right );

    public static bool operator!=( StaticBounds? left, StaticBounds? right ) => !Equals( left, right );

    #endregion

    public StaticBounds(
        IStaticFragment mapTile
    )
    {
        CenterLatitude = mapTile.Center.Latitude;
        CenterLongitude = mapTile.Center.Longitude;
        Height = mapTile.Height;
        Width = mapTile.Width;
    }

    public float CenterLatitude { get; }
    public float CenterLongitude { get; }
    public float Height { get; }
    public float Width { get; }
}
