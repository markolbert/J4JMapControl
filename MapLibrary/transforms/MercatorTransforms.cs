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

    public static double CartesianToLongitude( double x, double mapWidth )
    {
        if( mapWidth <= 0 )
            throw new ArgumentException( $"{nameof( CartesianToLongitude )}: Map width ({mapWidth}) must be > 0" );

        var halfWidth = mapWidth / 2;

        x = x < -halfWidth
            ? -halfWidth
            : x > halfWidth
                ? halfWidth
                : x;

        return 360 * x / mapWidth - 180;
    }

    public static double CartesianToLatitude( double y, double mapWidth )
    {
        if( mapWidth <= 0 )
            throw new ArgumentException( $"{nameof( CartesianToLatitude )}: Map width ({mapWidth}) must be > 0" );

        return (2 * Math.Atan( Math.Exp( TwoPi * y / mapWidth ) ) - HalfPi)/RadiansPerDegree;
    }

    public static LatLong CartesianToLatLong(
        double x,
        double y,
        double mapWidth
    ) =>
        new LatLong
        {
            Latitude = CartesianToLatitude( y, mapWidth ),
            Longitude = CartesianToLongitude( x, mapWidth )
        };

    public static double LongitudeToCartesian( double longitude, double mapWidth )
    {
        if( mapWidth <= 0 )
            throw new ArgumentException( $"{nameof( LongitudeToCartesian )}: Map width ({mapWidth}) must be > 0" );

        longitude = longitude < -180
            ? -180
            : longitude > 180
                ? 180
                : longitude;

        return mapWidth * ( longitude / 360 + 0.5 );
    }

    public static double LatitudeToCartesian( double latitude, double mapWidth )
    {
        if( mapWidth <= 0 )
            throw new ArgumentException( $"{nameof( LatitudeToCartesian )}: Map width ({mapWidth}) must be > 0" );

        var angleRadians = latitude * RadiansPerDegree;

        return mapWidth * Math.Log( Math.Tan( QuarterPi + angleRadians / 2 ) ) / TwoPi;
    }

    public static Vector<double> LatLongToCartesian(
        LatLong latLong,
        double mapWidth
    ) =>
        new Vector<double>( new[]
        {
            LatitudeToCartesian( latLong.Latitude, mapWidth ),
            LongitudeToCartesian( latLong.Longitude, mapWidth )
        } );
}
