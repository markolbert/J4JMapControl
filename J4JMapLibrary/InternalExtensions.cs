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

    internal static T ConformValueToRange<T>(T toCheck, MinMax<T> range, string name)
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

    internal static LatLong CartesianToLatLong(this ProjectionMetrics metrics, Cartesian cartesian)
    {
        // ReSharper disable once UseObjectOrCollectionInitializer
        var retVal = new LatLong(metrics);

        retVal.SetLatLong((2 * Math.Atan(
                               Math.Exp(MapConstants.TwoPi * cartesian.Y /
                                        (cartesian.Metrics.YRange.Maximum - cartesian.Metrics.YRange.Minimum)))
                           - MapConstants.HalfPi)
                          / MapConstants.RadiansPerDegree,
            360 * cartesian.X / (cartesian.Metrics.XRange.Maximum - cartesian.Metrics.XRange.Minimum) - 180);

        return retVal;
    }

    internal static Cartesian LatLongToCartesian( this ProjectionMetrics metrics, LatLong latLong )
    {
        var retVal = new Cartesian( metrics );

        var x = ( metrics.XRange.Maximum - metrics.XRange.Minimum ) * (latLong.Longitude / 360 + 0.5);

        var y = ( metrics.YRange.Maximum - metrics.YRange.Minimum )
          * Math.Log( Math.Tan( MapConstants.QuarterPi + latLong.Latitude * MapConstants.RadiansPerDegree / 2 ) )
          / MapConstants.TwoPi;

        try
        {
            retVal.SetCartesian(Convert.ToInt32(Math.Round(x)), Convert.ToInt32(Math.Round(y)));
        }
        catch (Exception ex)
        {
            Logger?.Error<string>( "Could not convert double to int32, message was '{0}'", ex.Message );
        }

        return retVal;
    }


}