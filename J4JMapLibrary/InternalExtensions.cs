using System.Numerics;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

internal static class InternalExtensions
{
    private static readonly IJ4JLogger? Logger;

    static InternalExtensions()
    {
        Logger = J4JDeusEx.GetLogger();
        Logger?.SetLoggedType( typeof( InternalExtensions ) );
    }

    internal static T ConformValueToRange<T>( this MinMax<T> range, T toCheck, string name )
        where T : struct, IComparable
    {
        if (toCheck.CompareTo(range.Minimum) < 0)
        {
            Logger?.Warning("{0} ({1}) < minimum ({2}), capping", name, toCheck, range.Minimum);
            return range.Minimum;
        }

        if (toCheck.CompareTo(range.Maximum) <= 0)
            return toCheck;

        Logger?.Warning("{0} ({1}) > maximum ({2}), capping", name, toCheck, range.Maximum);
        return range.Maximum;
    }

    internal static LatLong CartesianToLatLong(this ITiledMapScope scope, Cartesian cartesian )
    {
        // ReSharper disable once UseObjectOrCollectionInitializer
        var retVal = new LatLong(scope);

        retVal.SetLatLong((float) (2 * Math.Atan(
                               Math.Exp(MapConstants.TwoPi * cartesian.Y /
                                        (scope.YRange.Maximum - scope.YRange.Minimum)))
                           - MapConstants.HalfPi)
                          / MapConstants.RadiansPerDegree,
            360 * cartesian.X / (scope.XRange.Maximum - scope.XRange.Minimum) - 180);

        return retVal;
    }

    internal static Cartesian LatLongToCartesian( this ITiledMapScope scope, float latitude, float longitude ) =>
        scope
           .LatLongToCartesianInternal( scope.LatitudeRange.ConformValueToRange( latitude, "Latitude" ),
                                        scope.LongitudeRange
                                               .ConformValueToRange( longitude, "Longitude" ) );

    internal static Cartesian LatLongToCartesian( this ITiledMapScope scope, LatLong latLong ) =>
        scope.LatLongToCartesianInternal( latLong.Latitude, latLong.Longitude );

    private static Cartesian LatLongToCartesianInternal(this ITiledMapScope scope, float latitude, float longitude)
    {
        var retVal = new Cartesian(scope);

        var x = (scope.XRange.Maximum - scope.XRange.Minimum) * (longitude / 360 + 0.5);

        var y = (scope.YRange.Maximum - scope.YRange.Minimum)
          * Math.Log(Math.Tan(MapConstants.QuarterPi + latitude * MapConstants.RadiansPerDegree / 2))
          / MapConstants.TwoPi;

        try
        {
            retVal.SetCartesian(Convert.ToInt32(Math.Round(x)), Convert.ToInt32(Math.Round(y)));
        }
        catch (Exception ex)
        {
            Logger?.Error<string>("Could not convert float to int32, message was '{0}'", ex.Message);
        }

        return retVal;
    }

    internal static Vector3[] ApplyTransform( this Vector3[] points, Matrix4x4 transform )
    {
        var retVal = new Vector3[ points.Length ];

        for (var idx = 0; idx < points.Length; idx++)
        {
            retVal[idx] = Vector3.Transform(points[idx], transform);
        }

        return retVal;
    }
}