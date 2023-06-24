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
