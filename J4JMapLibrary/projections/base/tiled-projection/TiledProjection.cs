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

using J4JSoftware.Logging;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace J4JSoftware.J4JMapLibrary;

public abstract class TiledProjection<TAuth> : Projection<TAuth, IViewport, ITiledFragment>, ITiledProjection
    where TAuth : class, new()
{
    protected TiledProjection(
        IJ4JLogger logger
    )
        : base( logger )
    {
    }

    public ITileCache? TileCache { get; protected set; }

    public float GroundResolution( float latitude, int scale )
    {
        if( !Initialized )
        {
            Logger.Error( "Projection is not initialized" );
            return 0;
        }

        latitude = LatitudeRange.ConformValueToRange( latitude, "Latitude" );
        scale = ScaleRange.ConformValueToRange( scale, "Scale" );

        return (float) Math.Cos( latitude * MapConstants.RadiansPerDegree )
          * MapConstants.EarthCircumferenceMeters
          / GetHeightWidth( scale );
    }

    public string ScaleDescription( float latitude, int scale, float dotsPerInch ) =>
        $"1 : {GroundResolution( latitude, scale ) * dotsPerInch / MapConstants.MetersPerInch}";

#pragma warning disable CS1998
    public override async Task<bool> AuthenticateAsync( TAuth credentials, CancellationToken ctx = default )
#pragma warning restore CS1998
    {
        return !string.IsNullOrEmpty( Name );
    }

    public override async IAsyncEnumerable<TiledFragment> GetExtractAsync(
        IViewport viewportData,
        [ EnumeratorCancellation ] CancellationToken ctx = default
    )
    {
        if( !Initialized )
        {
            Logger.Error( "Projection not initialized" );
            yield break;
        }

        var scale = ScaleRange.ConformValueToRange( viewportData.Scale, "Viewport Scale" );

        var cartesianCenter = new TiledPoint(this) { Scale = viewportData.Scale };
        cartesianCenter.SetLatLong(viewportData.CenterLatitude, viewportData.CenterLongitude);
        var vpCenter = new Vector3(cartesianCenter.X, cartesianCenter.Y, 0);

        //var testRect = new Rectangle2D( viewportData.RequestedHeight,
        //                                viewportData.RequestedWidth,
        //                                viewportData.Rotation,
        //                                vpCenter,
        //                                CoordinateSystem2D.Display );

        var corner1 = new Vector3( cartesianCenter.X - viewportData.RequestedWidth / 2,
                                   cartesianCenter.Y + viewportData.RequestedHeight / 2,
                                   0 );
        var corner2 = new Vector3( corner1.X + viewportData.RequestedWidth, corner1.Y, 0 );
        var corner3 = new Vector3( corner2.X, corner2.Y - viewportData.RequestedHeight, 0 );
        var corner4 = new Vector3( corner1.X, corner3.Y, 0 );

        var corners = new[] { corner1, corner2, corner3, corner4 };

        // apply rotation if one is defined
        // heading == 270 is rotation == 90, hence the angle adjustment
        if( viewportData.Heading != 0 )
        {
            corners = corners.ApplyTransform(
                Matrix4x4.CreateRotationZ( ( 360 - viewportData.Heading ) * MapConstants.RadiansPerDegree, vpCenter ) );
        }

        // find the range of tiles covering the mapped rectangle
        var minTileX = CartesianToTile( corners.Min( x => x.X ) );
        var maxTileX = CartesianToTile( corners.Max( x => x.X ) );

        // figuring out the min/max of y coordinates is a royal pain in the ass...
        // because in display space, increasing y values take you >>down<< the screen,
        // not up the screen. So the first adjustment is to subject the raw Y values from
        // the height of the projection to reverse the direction. 
        var heightWidth = GetHeightWidth( scale );
        var minTileY = CartesianToTile( corners.Min( y => heightWidth - y.Y ) );
        var maxTileY = CartesianToTile( corners.Max( y => heightWidth - y.Y ) );

        minTileX = minTileX < 0 ? 0 : minTileX;
        minTileY = minTileY < 0 ? 0 : minTileY;

        var maxTiles = heightWidth / TileHeightWidth - 1;
        maxTileX = maxTileX > maxTiles ? maxTiles : maxTileX;
        maxTileY = maxTileY > maxTiles ? maxTiles : maxTileY;

        for( var xTile = minTileX; xTile <= maxTileX; xTile++ )
        {
            for( var yTile = minTileY; yTile <= maxTileY; yTile++ )
            {
                var mapTile = new TiledFragment( this, xTile, yTile, scale );

                //await TiledFragment.CreateAsync( this, xTile, yTile, ctx: ctx );

                if( TileCache != null )
                    mapTile.ImageData = await TileCache.GetImageDataAsync( mapTile, ctx );

                if( mapTile.ImageBytes <= 0 )
                    mapTile.ImageData = await mapTile.GetImageAsync( ctx );

                //await mapTile.GetImageAsync( ctx: ctx );

                yield return mapTile;
            }
        }
    }

    public override IMapFragment? GetFragment( int xTile, int yTile, int scale ) =>
        Task.Run( async () => await GetFragmentAsync( xTile, yTile, scale ) ).Result;

    public override async Task<IMapFragment?> GetFragmentAsync(int xTile, int yTile, int scale, CancellationToken ctx = default )
    {
        var tileXRange = GetTileXRange( scale );
        var tileYRange = GetTileYRange( scale );

        if( xTile < tileXRange.Minimum
        || xTile > tileXRange.Maximum
        || yTile < tileYRange.Minimum
        || yTile > tileYRange.Maximum )
            return null;

        var retVal = new TiledFragment( this, xTile, yTile, scale );

        if( TileCache != null )
            retVal.ImageData = await TileCache.GetImageDataAsync( retVal, ctx );

        if( retVal.ImageBytes <= 0 )
            retVal.ImageData = await retVal.GetImageAsync( ctx );

        return retVal;
    }

    private int CartesianToTile( float value ) => Convert.ToInt32( Math.Floor( value / TileHeightWidth ) );
}
