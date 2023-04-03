using System;
using System.Text;
using Windows.Foundation;
using J4JSoftware.DeusEx;
using J4JSoftware.J4JMapLibrary;
using Microsoft.UI.Xaml;
using Serilog;

namespace J4JSoftware.J4JMapWinLibrary;

public static class Extensions
{
    private static readonly string[] CardinalDirections = { "N", "North", "S", "South", "E", "East", "W", "West" };
    private static readonly ILogger? Logger;

    static Extensions()
    {
        Logger = J4JDeusEx.GetLogger();
        Logger?.ForContext( typeof( Extensions ) );
    }

    public static bool TryParseToLatLong( string? text, out float latitude, out float longitude )
    {
        latitude = 0;
        longitude = 0;

        if( string.IsNullOrEmpty( text ) )
            return false;

        var parts = text.Split( new[] { ',' } );
        if( parts.Length != 2 )
        {
            Logger?.Error( "Could not parse location text, missing ','" );
            return false;
        }

        if( !TryParseDirection( parts[ 0 ], out var dir1, out var dirType1 ) )
            return false;

        if( !TryParseDirection( parts[ 1 ], out var dir2, out var dirType2 ) )
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
                        latitude = dir1;
                        break;
                }

                break;

            case DirectionType.Latitude:
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch( dirType2 )
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
                switch( dirType2 )
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
            Logger?.Error( "Invalid latitude value ({0})", text );
            return false;
        }

        if( longitude >= -180 && longitude <= 180 )
            return true;

        Logger?.Error( "Invalid longitude value ({0})", text );
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

        Logger?.Error( "Could not parse direction string '{0}'", text );
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

    public static bool TryParseToPoint( string? text, out Point? point )
    {
        point = null;

        if( string.IsNullOrEmpty( text ) )
            return false;

        var parts = text.Split( ',', ' ' );
        if( parts.Length != 2 )
            return false;

        if( !float.TryParse( parts[ 0 ], out var xOffset ) )
            return false;

        if( !float.TryParse( parts[ 1 ], out var yOffset ) )
            return false;

        point = new Point( xOffset, yOffset );
        return true;
    }

    public static double AngleFromCenter( this FrameworkElement control, Point point ) =>
        AngleBetweenPoints( new Point( control.ActualWidth / 2, control.ActualHeight / 2 ), point );

    public static double AngleBetweenPoints( Point origin, Point point ) =>
        Math.Atan2( origin.Y - point.Y, point.X - origin.X )
      * MapConstants.DegreesPerRadian;

    private enum DirectionType
    {
        Latitude,
        Longitude,
        Unknown
    }
}
