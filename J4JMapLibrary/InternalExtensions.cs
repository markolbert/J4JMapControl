#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// InternalExtensions.cs
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

using System.Numerics;
using System.Text;

namespace J4JSoftware.J4JMapLibrary;

internal static class InternalExtensions
{
    // thanx to 3dGrabber for this
    // https://stackoverflow.com/questions/383587/how-do-you-do-integer-exponentiation-in-c
    internal static int Pow( int numBase, int exp ) =>
        Enumerable
           .Repeat( numBase, Math.Abs( exp ) )
           .Aggregate( 1, ( a, b ) => exp < 0 ? a / b : a * b );

    // key value matching is case sensitive
    internal static string ReplaceParameters(
        string template,
        Dictionary<string, string> values
    )
    {
        var sb = new StringBuilder( template );

        foreach( var kvp in values )
        {
            sb.Replace( kvp.Key, kvp.Value );
        }

        return sb.ToString();
    }

    internal static T ConformValueToRange<T>( this MinMax<T> range, T toCheck, string name )
        where T : struct, IComparable
    {
        if( toCheck.CompareTo( range.Minimum ) < 0 )
            return range.Minimum;

        if( toCheck.CompareTo( range.Maximum ) <= 0 )
            return toCheck;

        return range.Maximum;
    }

    internal static bool InRange<T>( this MinMax<T> range, T toCheck )
        where T : struct, IComparable =>
        toCheck.CompareTo( range.Minimum ) >= 0 && toCheck.CompareTo( range.Maximum ) <= 0;

    internal static int CompareTo<T>( this MinMax<T> range, T toCheck )
        where T : struct, IComparable
    {
        if( toCheck.CompareTo( range.Minimum ) < 0 )
            return -1;

        return toCheck.CompareTo( range.Maximum ) > 0 ? 1 : 0;
    }

    internal static Vector3[] ApplyTransform( this Vector3[] points, Matrix4x4 transform )
    {
        var retVal = new Vector3[ points.Length ];

        for( var idx = 0; idx < points.Length; idx++ )
        {
            retVal[ idx ] = Vector3.Transform( points[ idx ], transform );
        }

        return retVal;
    }
}
