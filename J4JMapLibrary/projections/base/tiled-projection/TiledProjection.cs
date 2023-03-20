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

using Serilog;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace J4JSoftware.J4JMapLibrary;

public abstract class TiledProjection<TAuth> : Projection<TAuth, IViewport, ITiledFragment>, ITiledProjection
    where TAuth : class, new()
{
    protected TiledProjection(
        ILogger logger
    )
        : base( logger )
    {
    }

    public ITileCache? TileCache { get; protected set; }

    public int HeightWidthInTiles( int scale )
    {
        scale = ScaleRange.ConformValueToRange( scale, "HeightWidthInTiles scale factor" );
        return InternalExtensions.Pow( 2, scale );
    }

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

    public override async IAsyncEnumerable<TiledFragment> GetViewportAsync(
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
        var leftTileX = CartesianToTile( corners.Min( x => x.X ) );
        var rightTileX = CartesianToTile( corners.Max( x => x.X ) );

        // figuring out the min/max of y coordinates is a royal pain in the ass...
        // because in display space, increasing y values take you >>down<< the screen,
        // not up the screen. So the first adjustment is to subject the raw Y values from
        // the height of the projection to reverse the direction. 
        //var heightWidth = GetHeightWidth( scale );
        var upperTileY = CartesianToTile( corners.Min( y => y.Y ) );
        var lowerTileY = CartesianToTile( corners.Max( y => y.Y ) );

        // define x range of tile coordinates so as to support wrapping around the
        // map/globe (this doesn't work for the y range because the upper and lower
        // edges of the map don't, and can't, correspond to the poles)
        //leftTileX = leftTileX < 0 ? 0 : leftTileX;
        //var maxTiles = HeightWidthInTiles( scale ) - 1;

        //var xTiles = Enumerable.Range( leftTileX, rightTileX - leftTileX + 1 ).ToList();
        //xTiles = xTiles.Select( x => x < 0 ? x + maxTiles + 1 : x > maxTiles ? x - maxTiles : x ).ToList();

        //var yTiles = Enumerable.Range( upperTileY, lowerTileY - upperTileY + 1 ).ToList();
        //yTiles = yTiles.Select( y => y < 0 ? 0 : y > maxTiles ? maxTiles : y ).ToList();

        //upperTileY = upperTileY < 0 ? 0 : upperTileY;

        //rightTileX = rightTileX > maxTiles ? maxTiles : rightTileX;
        //lowerTileY = lowerTileY > maxTiles ? maxTiles : lowerTileY;

        // as we iterate over tiles, keep track of which ones are "off the map",
        // and allow for rollover horizontally (we can't rollover vertically because
        // the upper and lower limits of the map don't include the poles)
        for( var xTile = leftTileX; xTile <= rightTileX; xTile++)
        {
            for( var yTile = upperTileY; yTile <= lowerTileY; yTile++ )
            {
                var mapTile = new TiledFragment( this, xTile, yTile, scale );

                if( mapTile.InProjection )
                {
                    if( TileCache != null )
                        mapTile.ImageData = await TileCache.GetImageDataAsync( mapTile, ctx );

                    if( mapTile.ImageBytes <= 0 )
                        mapTile.ImageData = await mapTile.GetImageAsync( ctx );
                }

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
