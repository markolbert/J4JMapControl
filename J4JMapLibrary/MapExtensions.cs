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

using System;
using System.Text;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;

namespace J4JSoftware.J4JMapLibrary;

public static class MapExtensions
{
    private static readonly IJ4JLogger? Logger;
    private static readonly char[] ValidQuadKeyCharacters = { '0', '1', '2', '3' };

    static MapExtensions()
    {
        Logger = J4JDeusEx.GetLogger();
        Logger?.SetLoggedType( typeof( MapExtensions ) );
    }

    public static ProjectionType GetProjectionType( this IProjection projection ) =>
        projection.GetType().GetInterface( nameof( ITiledProjection ) ) == null
            ? ProjectionType.Static
            : ProjectionType.Tiled;

    public static string GetQuadKey( this TiledFragment mapFragment, int scale )
    {
        var retVal = new StringBuilder();

        for( var i = scale; i > mapFragment.MapServer.ScaleRange.Minimum - 1; i-- )
        {
            var digit = '0';
            var mask = 1 << ( i - 1 );

            if( ( mapFragment.X & mask ) != 0 )
                digit++;

            if( ( mapFragment.Y & mask ) != 0 )
            {
                digit++;
                digit++;
            }

            retVal.Append( digit );
        }

        return retVal.ToString();
    }

    public static string? GetQuadKey( this ITiledProjection projection, int xTile, int yTile, int scale )
    {
        var x = projection.TileXRange.ConformValueToRange( xTile, "X Tile" );
        var y = projection.TileYRange.ConformValueToRange( yTile, "Y Tile" );

        if( x != xTile || y != yTile )
        {
            Logger?.Error( "Tile coordinates ({0}, {1}) are inconsistent with projection's scale {2}",
                           xTile,
                           yTile,
                           scale );

            return null;
        }

        var retVal = new StringBuilder();

        for( var i = scale; i > 0; i-- )
        {
            var digit = '0';
            var mask = 1 << ( i - 1 );

            if( ( xTile & mask ) != 0 )
                digit++;

            if( ( yTile & mask ) != 0 )
            {
                digit++;
                digit++;
            }

            retVal.Append( digit );
        }

        return retVal.ToString();
    }

    public static bool TryParseQuadKey( string quadKey, out DeconstructedQuadKey? result )
    {
        result = null;

        if( quadKey.Length < 1 )
        {
            Logger?.Error( "Empty quadkey string" );
            return false;
        }

        result = new DeconstructedQuadKey { Scale = quadKey.Length };

        if( !quadKey.Any( x => ValidQuadKeyCharacters.Any( y => y == x ) ) )
        {
            Logger?.Error( "Invalid characters in quadkey string" );
            return false;
        }

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

                default:
                    Logger?.Error( "Unexpected quadkey character '{0}'", quadKey[ levelOfDetail - 1 ] );
                    break;
            }
        }

        return true;
    }

    public static string LatitudeToText( float value, int decimals = 5 )
    {
        if( decimals < 0 )
        {
            Logger?.Error( "Trying to set number of decimal digits < 0, defaulting to 5" );
            decimals = 5;
        }

        var ns = value < 0 ? "S" : "N";

        var decValue = Math.Round( (decimal) value, decimals );

        var (whole, fraction) = GetWholeFraction( decValue );

        return $"{whole}x{fraction}{ns}";
    }

    public static string LongitudeToText( float value, int decimals = 5 )
    {
        if( decimals < 0 )
        {
            Logger?.Error( "Trying to set number of decimal digits < 0, defaulting to 5" );
            decimals = 5;
        }

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
}
