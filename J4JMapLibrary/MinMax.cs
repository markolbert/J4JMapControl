#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// MinMax.cs
//
// This file is part of JumpForJoy Software's J4JMapLibrary.
// 
// J4JMapLibrary is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JMapLibrary is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JMapLibrary. If not, see <https://www.gnu.org/licenses/>.

#endregion

namespace J4JSoftware.J4JMapLibrary;

public class MinMax<T> : IEquatable<MinMax<T>>
    where T : struct, IComparable
{
    public MinMax(
        T min,
        T max
    )
    {
        if( min.CompareTo( max ) > 0 )
        {
            // min/max are reversed
            Minimum = max;
            Maximum = min;
        }
        else
        {
            // min/max are in correct sequence
            Minimum = min;
            Maximum = max;
        }
    }

    public T Minimum { get; }
    public T Maximum { get; }

    public bool Equals( MinMax<T>? other )
    {
        if( ReferenceEquals( null, other ) ) return false;
        if( ReferenceEquals( this, other ) ) return true;

        return Minimum.Equals( other.Minimum ) && Maximum.Equals( other.Maximum );
    }

    public override bool Equals( object? obj )
    {
        if( ReferenceEquals( null, obj ) ) return false;
        if( ReferenceEquals( this, obj ) ) return true;
        if( obj.GetType() != GetType() ) return false;

        return Equals( (MinMax<T>) obj );
    }

    public override int GetHashCode() => HashCode.Combine( Minimum, Maximum );

    public static bool operator==( MinMax<T>? left, MinMax<T>? right ) => Equals( left, right );

    public static bool operator!=( MinMax<T>? left, MinMax<T>? right ) => !Equals( left, right );
}
