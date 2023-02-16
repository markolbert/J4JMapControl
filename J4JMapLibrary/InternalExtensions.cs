using System.Numerics;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;
using System.Text;

namespace J4JMapLibrary;

internal static class InternalExtensions
{
    private static readonly IJ4JLogger? Logger;

    static InternalExtensions()
    {
        Logger = J4JDeusEx.GetLogger();
        Logger?.SetLoggedType( typeof( InternalExtensions ) );
    }

    // thanx to 3dGrabber for this
    // https://stackoverflow.com/questions/383587/how-do-you-do-integer-exponentiation-in-c
    internal static int Pow( int numBase, int exp ) =>
        Enumerable
           .Repeat( numBase, Math.Abs( exp ) )
           .Aggregate( 1, ( a, b ) => exp < 0 ? a / b : a * b );

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

    internal static T ConformValueToRange<T>( this MinMax<T> range, T toCheck, string name )
        where T : struct, IComparable
    {
        if( toCheck.CompareTo( range.Minimum ) < 0 )
        {
            Logger?.Warning( "{0} ({1}) < minimum ({2}), capping", name, toCheck, range.Minimum );
            return range.Minimum;
        }

        if( toCheck.CompareTo( range.Maximum ) <= 0 )
            return toCheck;

        Logger?.Warning( "{0} ({1}) > maximum ({2}), capping", name, toCheck, range.Maximum );
        return range.Maximum;
    }

    internal static Cartesian LatLongToCartesian( this ITiledScale scale, float latitude, float longitude ) =>
        scale.LatLongToCartesianInternal( scale.MapServer.LatitudeRange.ConformValueToRange( latitude, "Latitude" ),
                                          scale.MapServer.LongitudeRange
                                               .ConformValueToRange( longitude, "Longitude" ) );

    internal static LatLong CartesianToLatLong( this ITiledScale scale, Cartesian cartesian )
    {
        // ReSharper disable once UseObjectOrCollectionInitializer
        var retVal = new LatLong( scale.MapServer );

        retVal.SetLatLong( (float) ( 2
                             * Math.Atan( Math.Exp( MapConstants.TwoPi
                                                  * cartesian.Y
                                                  / ( scale.YRange.Maximum - scale.YRange.Minimum ) ) )
                             - MapConstants.HalfPi )
                         / MapConstants.RadiansPerDegree,
                           360 * cartesian.X / ( scale.XRange.Maximum - scale.XRange.Minimum ) - 180 );

        return retVal;
    }

    internal static Cartesian LatLongToCartesian( this ITiledScale scale, LatLong latLong ) =>
        scale.LatLongToCartesianInternal( latLong.Latitude, latLong.Longitude );

    private static Cartesian LatLongToCartesianInternal( this ITiledScale scale, float latitude, float longitude )
    {
        var retVal = new Cartesian( scale );

        var width = scale.XRange.Maximum - scale.XRange.Minimum + 1;
        var x = width * ( longitude / 360 + 0.5 );

        var height = scale.YRange.Maximum - scale.YRange.Minimum + 1;

        // this weird "subtract the calculation from half the height" is due to the
        // fact y values increase going >>down<< the display, so the top is y = 0
        // while the bottom is y = height
        var y = height / 2F
          - height
          * Math.Log( Math.Tan( MapConstants.QuarterPi + latitude * MapConstants.RadiansPerDegree / 2 ) )
          / MapConstants.TwoPi;

        try
        {
            retVal.SetCartesian( Convert.ToInt32( Math.Round( x ) ), Convert.ToInt32( Math.Round( y ) ) );
        }
        catch( Exception ex )
        {
            Logger?.Error<string>( "Could not convert float to int32, message was '{0}'", ex.Message );
        }

        return retVal;
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
