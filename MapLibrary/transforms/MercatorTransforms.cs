using System.ComponentModel;

namespace J4JSoftware.MapLibrary;

public static class MercatorTransforms
{
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
            AngleMeasure.Radians => 2 * Math.PI * x / mapWidth,
            AngleMeasure.Degrees => 360 * x / mapWidth,
            _ => throw new InvalidEnumArgumentException(
                $"{nameof( CartesianToLongitude )}: unsupported {typeof( AngleMeasure )} value '{angleMeasure}'" )
        };
    }

    public static double CartesianToLongitude(
        this IMapProjection mapProjection,
        double x,
        AngleMeasure angleMeasure = AngleMeasure.Degrees
    ) =>
        CartesianToLongitude( x, mapProjection.ProjectionWidthHeight, angleMeasure );

    public static double CartesianToLatitude(
        double y,
        double mapWidth,
        AngleMeasure angleMeasure = AngleMeasure.Degrees
    )
    {
        if( mapWidth <= 0 )
            throw new ArgumentException( $"{nameof( CartesianToLatitude )}: Map width ({mapWidth}) must be > 0" );

        var retVal = 2 * Math.Atan( Math.Exp( 2 * Math.PI * y / mapWidth ) ) - Math.PI / 2;

        return angleMeasure switch
        {
            AngleMeasure.Radians => retVal,
            AngleMeasure.Degrees => retVal * 180 / Math.PI,
            _ => throw new InvalidEnumArgumentException(
                $"{nameof( CartesianToLatitude )}: unsupported {typeof( AngleMeasure )} value '{angleMeasure}'" )
        };
    }

    public static double CartesianToLatitude(
        this IMapProjection mapProjection,
        double y,
        AngleMeasure angleMeasure = AngleMeasure.Degrees
    ) =>
        CartesianToLatitude( y, mapProjection.ProjectionWidthHeight, angleMeasure );

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
            AngleMeasure.Radians => angle * 180 / Math.PI,
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

    public static double LongitudeToCartesian(
        this IMapProjection mapProjection,
        double angle,
        AngleMeasure angleMeasure = AngleMeasure.Degrees
    ) =>
        LongitudeToCartesian( angle, mapProjection.ProjectionWidthHeight, angleMeasure );

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
            AngleMeasure.Degrees => angle * Math.PI / 180,
            _ => throw new InvalidEnumArgumentException(
                $"{nameof( LatitudeToCartesian )}: unsupported {typeof( AngleMeasure )} value '{angleMeasure}'" )
        };

        return mapWidth * Math.Log( Math.Tan( Math.PI / 4 + angleRadians / 2 ) ) / ( 2 * Math.PI );
    }

    public static double LatitudeToCartesian(
        this IMapProjection mapProjection,
        double angle,
        AngleMeasure angleMeasure = AngleMeasure.Degrees
    ) =>
        LatitudeToCartesian( angle, mapProjection.ProjectionWidthHeight, angleMeasure );
}
