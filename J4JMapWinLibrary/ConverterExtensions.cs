using System;
using System.Text;
using J4JSoftware.DeusEx;
using Serilog;

namespace J4JSoftware.J4JMapWinLibrary;

internal static class ConverterExtensions
{
    private enum DirectionType
    {
        Latitude,
        Longitude,
        Unknown
    }

    private static readonly string[] CardinalDirections = { "N", "North", "S", "South", "E", "East", "W", "West" };
    private static readonly ILogger? Logger;

    static ConverterExtensions()
    {
        Logger = J4JDeusEx.GetLogger();
        Logger?.ForContext( typeof( ConverterExtensions ) );
    }

    public static bool TryParseToLatLong( string? text, out float latitude, out float longitude )
    {
        latitude = float.MinValue;
        longitude = float.MinValue;

        if( string.IsNullOrEmpty( text ) )
            return false;

        var parts = text.Split( new [] { ',' } );
        if( parts.Length != 2 )
        {
            Logger?.Error("Could not parse location text, missing ','"  );
            return false;
        }

        if( !TryParseDirection( parts[ 0 ], out var dir1, out var dirType1 ) )
            return false;

        if (!TryParseDirection(parts[1], out var dir2, out var dirType2))
            return false;

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch( dirType1 )
        {
            case DirectionType.Unknown:
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch( dirType2 )
                {
                    case DirectionType.Unknown:
                        latitude = dir1;
                        longitude = dir2;
                        break;

                    case DirectionType.Latitude:
                        latitude = dir2;
                        longitude = dir1;
                        break;

                    case DirectionType.Longitude: 
                        longitude = dir2;
                        latitude =dir1;
                        break;
                }

                break;

            case DirectionType.Latitude:
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (dirType2)
                {
                    case DirectionType.Latitude:
                        // can't both be latitude!
                        break;

                    default:
                        longitude = dir2;
                        latitude = dir1;
                        break;
                }

                break;


            case DirectionType.Longitude:
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (dirType2)
                {
                    case DirectionType.Longitude:
                        // can't both be longitude!
                        break;

                    default:
                        longitude = dir1;
                        latitude = dir2;
                        break;
                }

                break;
        }

        if( latitude is < -90 or > 90 )
        {
            Logger?.Error("Invalid latitude value ({0})", text  );
            return false;
        }

        if( longitude >= -180 && longitude <= 180 )
            return true;

        Logger?.Error("Invalid longitude value ({0})", text);
        return false;
    }

    private static bool TryParseDirection( string? text, out float direction, out DirectionType dirType )
    {
        direction = float.MinValue;
        dirType = DirectionType.Unknown;

        if( string.IsNullOrEmpty( text ) )
            return false;

        var sign = 1;
        text = text.ToUpper();

        foreach( var cardinal in CardinalDirections )
        {
            var index = text.IndexOf( cardinal, StringComparison.OrdinalIgnoreCase );
            if( index < 0 )
                continue;

            sign = cardinal switch
            {
                "N" => 1,
                "NORTH" => 1,
                "E" => 1,
                "EAST" => 1,
                _ => -1
            };

            dirType = cardinal switch
            {
                "N" => DirectionType.Latitude,
                "NORTH" => DirectionType.Latitude,
                "S" => DirectionType.Latitude,
                "SOUTH" => DirectionType.Latitude,
                "E" => DirectionType.Longitude,
                "EAST" => DirectionType.Longitude,
                "W" => DirectionType.Longitude,
                "WEST" => DirectionType.Longitude,
                _ => DirectionType.Unknown
            };

            text = text.Replace( cardinal, string.Empty );
            break;
        }

        if( float.TryParse( text, out direction ) )
        {
            direction *= sign;
            return true;
        }

        Logger?.Error("Could not parse direction string '{0}'", text  );
        return false;
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
}
