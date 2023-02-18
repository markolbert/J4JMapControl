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

public abstract class TiledProjection<TAuth> : Projection<TAuth, IViewport, TiledFragment>, ITiledProjection
    where TAuth : class
{
    protected TiledProjection(
        IJ4JLogger logger,
        ITileCache? tileCache
    )
        : base( logger )
    {
        TileCache = tileCache;
        TileXRange = new MinMax<int>( 0, 0 );
        TileYRange = new MinMax<int>( 0, 0 );
    }

    protected TiledProjection(
        IProjectionCredentials credentials,
        IJ4JLogger logger,
        ITileCache? tileCache
    )
        : base( credentials, logger )
    {
        TileCache = tileCache;
        TileXRange = new MinMax<int>( 0, 0 );
        TileYRange = new MinMax<int>( 0, 0 );
    }

    public ITiledScale? TiledScale { get; protected set; }
    public abstract int TileHeightWidth { get; }

    public override IProjectionScale MapScale =>
        TiledScale ?? throw new NullReferenceException( $"{nameof( TiledScale )} was not initialized" );

    public int Height => TiledScale == null ? 0 : TiledScale.YRange.Maximum - TiledScale.YRange.Minimum + 1;
    public int Width => TiledScale == null ? 0 : TiledScale.XRange.Maximum - TiledScale.XRange.Minimum + 1;

    public ITileCache? TileCache { get; }

    public MinMax<int> TileXRange { get; }
    public MinMax<int> TileYRange { get; }

    public float GroundResolution( float latitude )
    {
        if( !Initialized )
        {
            Logger.Error( "Projection is not initialized" );
            return 0;
        }

        latitude = MapServer.LatitudeRange.ConformValueToRange( latitude, "Latitude" );

        return (float) Math.Cos( latitude * MapConstants.RadiansPerDegree )
          * MapConstants.EarthCircumferenceMeters
          / Width;
    }

    public string ScaleDescription( float latitude, float dotsPerInch ) =>
        $"1 : {GroundResolution( latitude ) * dotsPerInch / MapConstants.MetersPerInch}";

    public override async IAsyncEnumerable<TiledFragment> GetExtractAsync(
        IViewport viewportData,
        bool deferImageLoad = false,
        [ EnumeratorCancellation ] CancellationToken ctx = default
    )
    {
        if( !Initialized )
        {
            Logger.Error( "Projection not initialized" );
            yield break;
        }

        TiledScale!.Scale = viewportData.Scale;

        var cartesianCenter = new Cartesian( TiledScale );
        cartesianCenter.SetCartesian(
            TiledScale.LatLongToCartesian( viewportData.CenterLatitude, viewportData.CenterLongitude ) );

        var corner1 = new Vector3( cartesianCenter.X - viewportData.RequestedWidth / 2,
                                   cartesianCenter.Y + viewportData.RequestedHeight / 2,
                                   0 );
        var corner2 = new Vector3( corner1.X + viewportData.RequestedWidth, corner1.Y, 0 );
        var corner3 = new Vector3( corner2.X, corner2.Y - viewportData.RequestedHeight, 0 );
        var corner4 = new Vector3( corner1.X, corner3.Y, 0 );

        var corners = new[] { corner1, corner2, corner3, corner4 };

        var vpCenter = new Vector3( cartesianCenter.X, cartesianCenter.Y, 0 );

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
        var minTileY = CartesianToTile( corners.Min( y => Height - y.Y ) );
        var maxTileY = CartesianToTile( corners.Max( y => Height - y.Y ) );

        minTileX = minTileX < 0 ? 0 : minTileX;
        minTileY = minTileY < 0 ? 0 : minTileY;

        var maxTiles = Height / MapServer.TileHeightWidth - 1;
        maxTileX = maxTileX > maxTiles ? maxTiles : maxTileX;
        maxTileY = maxTileY > maxTiles ? maxTiles : maxTileY;

        for( var xTile = minTileX; xTile <= maxTileX; xTile++ )
        {
            for( var yTile = minTileY; yTile <= maxTileY; yTile++ )
            {
                var mapTile = await TiledFragment.CreateAsync( this, xTile, yTile, ctx: ctx );

                if( !deferImageLoad )
                    await mapTile.GetImageAsync( ctx: ctx );

                yield return mapTile;
            }
        }
    }

    private int CartesianToTile( float value ) => Convert.ToInt32( Math.Floor( value / MapServer.TileHeightWidth ) );
}
