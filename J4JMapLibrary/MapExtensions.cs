#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// MapExtensions.cs
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

using System.Text;
using System.Text.RegularExpressions;

namespace J4JSoftware.J4JMapLibrary;

public static class MapExtensions
{
    private static readonly char[] ValidQuadKeyCharacters = { '0', '1', '2', '3' };
    private static readonly Regex LatLongRegEx = new( "^\\s*((-?[0-9]*\\.?)?[0-9]+)(\\D*)$", RegexOptions.Compiled );
    private static readonly string[] CardinalDirections = { "N", "North", "S", "South", "E", "East", "W", "West" };

    public static ProjectionType GetProjectionType( this IProjection projection ) =>
        projection.GetType().GetInterface( nameof( ITiledProjection ) ) == null
            ? ProjectionType.Static
            : ProjectionType.Tiled;

    public static bool TryParseQuadKey( string quadKey, out DeconstructedQuadKey? result )
    {
        result = null;

        if( quadKey.Length < 1 )
            return false;

        result = new DeconstructedQuadKey { Scale = quadKey.Length };

        if( !quadKey.Any( x => ValidQuadKeyCharacters.Any( y => y == x ) ) )
            return false;

        var levelOfDetail = quadKey.Length;

        for( var i = levelOfDetail; i > 0; i-- )
        {
            var mask = 1 << ( i - 1 );
            switch( quadKey[ levelOfDetail - i ] )
            {
                case '0':
                    break;

                case '1':
                    result.XTile |= mask;
                    break;

                case '2':
                    result.YTile |= mask;
                    break;

                case '3':
                    result.XTile |= mask;
                    result.YTile |= mask;
                    break;
            }
        }

        return true;
    }

    public static string LatitudeToText( float value, int decimals = 5 )
    {
        if( decimals < 0 )
            decimals = 5;

        var ns = value < 0 ? "S" : "N";

        var decValue = Math.Round( (decimal) value, decimals );

        var (whole, fraction) = GetWholeFraction( decValue );

        return $"{whole}x{fraction}{ns}";
    }

    public static string LongitudeToText( float value, int decimals = 5 )
    {
        if( decimals < 0 )
            decimals = 5;

        var ew = value < 0 ? "W" : "E";

        var decValue = Math.Round( (decimal) value, decimals );

        var (whole, fraction) = GetWholeFraction( decValue );

        return $"{whole}x{fraction}{ew}";
    }

    private static (int Whole, string Fraction) GetWholeFraction( decimal value )
    {
        value = Math.Abs( value );
        var whole = (int) Math.Floor( value );

        var fraction = $"{value - whole}";
        var decLog = fraction.IndexOf( '.' );

        return ( whole, fraction[ ( decLog + 1 ).. ] );
    }

    public static bool TryParseLatitudeDirection( string? text, out int sign )
    {
        if( !string.IsNullOrEmpty( text ) )
            text = text.Trim().ToUpper();

        sign = text switch
        {
            "N" => 1,
            "NORTH" => 1,
            "S" => -1,
            "SOUTH" => -1,
            _ => 0
        };

        return sign != 0;
    }

    public static bool TryParseLongitudeDirection( string? text, out int sign )
    {
        if( !string.IsNullOrEmpty( text ) )
            text = text.Trim().ToUpper();

        sign = text switch
        {
            "E" => 1,
            "EAST" => 1,
            "W" => -1,
            "WEST" => -1,
            _ => 0
        };

        return sign != 0;
    }

    public static bool TryParseToLatitude( string? text, out float latitude )
    {
        latitude = 0;

        if( string.IsNullOrEmpty( text ) )
            return false;

        var results = LatLongRegEx.Matches( text );
        if( !results.Any() )
            return false;

        if( !float.TryParse( results[ 0 ].Groups[ 1 ].Value, out latitude ) )
            return false;

        latitude = MapConstants.Wgs84LatitudeRange.ConformValueToRange( latitude, "TryParseToLatitude" );

        if( latitude <= 0 || results[ 0 ].Groups.Count < 4 )
            return true;

        if( !TryParseLatitudeDirection( results[ 0 ].Groups[ 3 ].Value, out var sign ) )
            return string.IsNullOrEmpty( results[ 0 ].Groups[ 3 ].Value );

        latitude *= sign;
        return true;
    }

    public static bool TryParseToLongitude( string? text, out float longitude )
    {
        longitude = 0;

        if( string.IsNullOrEmpty( text ) )
            return false;

        var results = LatLongRegEx.Matches( text );
        if( !results.Any() )
            return false;

        if( !float.TryParse( results[ 0 ].Groups[ 1 ].Value, out longitude ) )
            return false;

        longitude = MapConstants.LongitudeRange.ConformValueToRange( longitude, "TryParseToLongitude" );

        if( longitude <= 0 || results[ 0 ].Groups.Count < 4 )
            return true;

        if( !TryParseLongitudeDirection( results[ 0 ].Groups[ 3 ].Value, out var sign ) )
            return string.IsNullOrEmpty( results[ 0 ].Groups[ 3 ].Value );

        longitude *= sign;
        return true;
    }

    public static bool TryParseToLatLong( string? text, out float latitude, out float longitude )
    {
        latitude = 0;
        longitude = 0;

        if( string.IsNullOrEmpty( text ) )
            return false;

        var parts = text.Split( new[] { ',' } );
        if( parts.Length != 2 )
            return false;

        return TryParseToLatitude( parts[ 0 ], out latitude ) && TryParseToLongitude( parts[ 1 ], out longitude );
    }

    public static string ConvertToLatLongText( float latitude, float longitude )
    {
        var sb = new StringBuilder();
        sb.Append( latitude );
        sb.Append( latitude < 0 ? "S, " : "N, " );
        sb.Append( longitude );
        sb.Append( longitude < 0 ? "W" : "E" );

        return sb.ToString();
    }

    public static bool TryParseHeading( string text, out double heading )
    {
        heading = text.ToLower().Trim() switch
        {
            "n" => 0D,
            "e" => -90D,
            "s" => 180D,
            "w" => 90D,
            "ne" => -45D,
            "se" => -135D,
            "sw" => 135D,
            "nw" => 45D,
            "nne" => -22.5,
            "ene" => -67.5,
            "ese" => -112.5,
            "sse" => -157.5,
            "ssw" => 157.5,
            "wsw" => 112.5,
            "wnw" => 67.5,
            "nnw" => 22.5,
            _ => double.NaN
        };

        return !double.IsNaN( heading );
    }
}
