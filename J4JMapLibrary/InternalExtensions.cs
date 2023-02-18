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

using System.Numerics;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;
using System.Text;

namespace J4JSoftware.J4JMapLibrary;

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

    internal static LatLong TiledCartesianToLatLong( this ITiledProjection projection, TiledCartesian tiledCartesian )
    {
        // ReSharper disable once UseObjectOrCollectionInitializer
        var retVal = new LatLong( projection.MapServer );

        retVal.SetLatLong( (float) ( 2
                             * Math.Atan( Math.Exp( MapConstants.TwoPi
                                                  * tiledCartesian.Y
                                                  / projection.TileHeightWidth ) )
                             - MapConstants.HalfPi )
                         / MapConstants.RadiansPerDegree,
                           360 * tiledCartesian.X / projection.TileHeightWidth - 180 );

        return retVal;
    }

    //internal static TiledCartesian LatLongToTiledCartesian(this ITiledScale scale, float latitude, float longitude) =>
    //    scale.LatLongToCartesianInternal(scale.MapServer.LatitudeRange.ConformValueToRange(latitude, "Latitude"),
    //                                     scale.MapServer.LongitudeRange
    //                                          .ConformValueToRange(longitude, "Longitude"));

    internal static TiledCartesian LatLongToTiledCartesian(this ITiledProjection projection, LatLong latLong) =>
        projection.LatLongToTiledCartesian(latLong.Latitude, latLong.Longitude);

    //internal static TiledCartesian LatLongToTiledCartesian( this ITiledScale scale, LatLong latLong ) =>
    //    scale.LatLongToCartesianInternal( latLong.Latitude, latLong.Longitude );

    internal static TiledCartesian LatLongToTiledCartesian(this ITiledProjection projection, float latitude, float longitude)
    {
        latitude = projection.MapServer.LatitudeRange.ConformValueToRange( latitude, "Latitude" );
        longitude = projection.MapServer.LongitudeRange.ConformValueToRange( longitude, "Longitude" );

        var retVal = new TiledCartesian(projection);

        var x = projection.TileHeightWidth * (longitude / 360 + 0.5);

        // this weird "subtract the calculation from half the height" is due to the
        // fact y values increase going >>down<< the display, so the top is y = 0
        // while the bottom is y = height
        var y = projection.TileHeightWidth / 2F
          - projection.TileHeightWidth
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

    //private static TiledCartesian LatLongToCartesianInternal( this ITiledScale scale, float latitude, float longitude )
    //{
    //    var retVal = new TiledCartesian( scale );

    //    var width = scale.XRange.Maximum - scale.XRange.Minimum + 1;
    //    var x = width * ( longitude / 360 + 0.5 );

    //    var height = scale.YRange.Maximum - scale.YRange.Minimum + 1;

    //    // this weird "subtract the calculation from half the height" is due to the
    //    // fact y values increase going >>down<< the display, so the top is y = 0
    //    // while the bottom is y = height
    //    var y = height / 2F
    //      - height
    //      * Math.Log( Math.Tan( MapConstants.QuarterPi + latitude * MapConstants.RadiansPerDegree / 2 ) )
    //      / MapConstants.TwoPi;

    //    try
    //    {
    //        retVal.SetCartesian( Convert.ToInt32( Math.Round( x ) ), Convert.ToInt32( Math.Round( y ) ) );
    //    }
    //    catch( Exception ex )
    //    {
    //        Logger?.Error<string>( "Could not convert float to int32, message was '{0}'", ex.Message );
    //    }

    //    return retVal;
    //}

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
