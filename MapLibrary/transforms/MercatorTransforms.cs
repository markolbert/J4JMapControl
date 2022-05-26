using System;
using System.ComponentModel;
using System.Numerics;

namespace J4JSoftware.MapLibrary;

public static class MercatorTransforms
{
    public const double RadiansPerDegree = Math.PI / 180;
    public const double TwoPi = 2 * Math.PI;
    public const double HalfPi = Math.PI / 2;
    public const double QuarterPi = Math.PI / 4;

    public static double CartesianToLongitude(
        double x,
        double mapWidth,
        AngleMeasure angleMeasure = AngleMeasure.Degrees
    )
    {
        if( mapWidth <= 0 )
            throw new ArgumentException( $"{nameof( CartesianToLongitude )}: Map width ({mapWidth}) must be > 0" );

        var halfWidth = mapWidth / 2;

        x = x < -halfWidth
            ? -halfWidth
            : x > halfWidth
                ? halfWidth
                : x;

        return angleMeasure switch
        {
            AngleMeasure.Radians => TwoPi * x / mapWidth - Math.PI,
            AngleMeasure.Degrees => 360 * x / mapWidth - 180,
            _ => throw new InvalidEnumArgumentException(
                $"{nameof( CartesianToLongitude )}: unsupported {typeof( AngleMeasure )} value '{angleMeasure}'" )
        };
    }

    public static double CartesianToLatitude(
        double y,
        double mapWidth,
        AngleMeasure angleMeasure = AngleMeasure.Degrees
    )
    {
        if( mapWidth <= 0 )
            throw new ArgumentException( $"{nameof( CartesianToLatitude )}: Map width ({mapWidth}) must be > 0" );

        var retVal = 2 * Math.Atan( Math.Exp( TwoPi * y / mapWidth ) ) - HalfPi;

        return angleMeasure switch
        {
            AngleMeasure.Radians => retVal,
            AngleMeasure.Degrees => retVal / RadiansPerDegree,
            _ => throw new InvalidEnumArgumentException(
                $"{nameof( CartesianToLatitude )}: unsupported {typeof( AngleMeasure )} value '{angleMeasure}'" )
        };
    }

    public static LatLong CartesianToLatLong(
        double x,
        double y,
        double mapWidth,
        AngleMeasure angleMeasure = AngleMeasure.Degrees
    ) =>
        new LatLong
        {
            Latitude = CartesianToLatitude( y, mapWidth, angleMeasure ),
            Longitude = CartesianToLongitude( x, mapWidth, angleMeasure )
        };

    public static double LongitudeToCartesian(
        double angle,
        double mapWidth,
        AngleMeasure angleMeasure = AngleMeasure.Degrees
    )
    {
        if( mapWidth <= 0 )
            throw new ArgumentException( $"{nameof( LongitudeToCartesian )}: Map width ({mapWidth}) must be > 0" );

        var angleDegrees = angleMeasure switch
        {
            AngleMeasure.Radians => angle / RadiansPerDegree,
            AngleMeasure.Degrees => angle,
            _ => throw new InvalidEnumArgumentException(
                $"{nameof( LongitudeToCartesian )}: unsupported {typeof( AngleMeasure )} value '{angleMeasure}'" )
        };

        angleDegrees = angleDegrees < -180
            ? -180
            : angle > 180
                ? 180
                : angle;

        return mapWidth * ( angleDegrees / 360 + 0.5 );
    }

    public static double LatitudeToCartesian(
        double angle,
        double mapWidth,
        AngleMeasure angleMeasure = AngleMeasure.Degrees
    )
    {
        if( mapWidth <= 0 )
            throw new ArgumentException( $"{nameof( LatitudeToCartesian )}: Map width ({mapWidth}) must be > 0" );

        var angleRadians = angleMeasure switch
        {
            AngleMeasure.Radians => angle,
            AngleMeasure.Degrees => angle * RadiansPerDegree,
            _ => throw new InvalidEnumArgumentException(
                $"{nameof( LatitudeToCartesian )}: unsupported {typeof( AngleMeasure )} value '{angleMeasure}'" )
        };

        return mapWidth * Math.Log( Math.Tan( QuarterPi + angleRadians / 2 ) ) / TwoPi;
    }

    public static Vector<double> LatLongToCartesian(
        LatLong latLong,
        double mapWidth,
        AngleMeasure angleMeasure = AngleMeasure.Degrees
    ) =>
        new Vector<double>( new[]
        {
            LatitudeToCartesian( latLong.Latitude, mapWidth, angleMeasure ),
            LongitudeToCartesian( latLong.Longitude, mapWidth, angleMeasure )
        } );
}
