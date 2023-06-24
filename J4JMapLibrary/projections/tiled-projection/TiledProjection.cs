#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// TiledProjection.cs
//
// This file is part of JumpForJoy Software's J4JMapLibrary.
// 
// J4JMapLibrary is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JMapLibrary is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JMapLibrary. If not, see <https://www.gnu.org/licenses/>.

#endregion

using J4JSoftware.VisualUtilities;
using Microsoft.Extensions.Logging;
using System.Numerics;
using System.Runtime.ConstrainedExecution;

namespace J4JSoftware.J4JMapLibrary;

public abstract class TiledProjection : Projection, ITiledProjection
{
    protected TiledProjection(
        IEnumerable<string>? mapStyles = null,
        ILoggerFactory? loggerFactory = null
    )
        : base( mapStyles, loggerFactory )
    {
    }

    public ITileCaching TileCaching { get; } = new TileCaching();

    public int HeightWidthInTiles( int scale )
    {
        scale = ScaleRange.ConformValueToRange( scale, "HeightWidthInTiles scale factor" );
        return MapExtensions.Pow( 2, scale );
    }

    public float GroundResolution( float latitude, int scale )
    {
        if( !Initialized )
        {
            Logger?.LogError( "Projection is not initialized" );
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

    public override async Task<MapBlock?> GetMapTileAsync(
        int xTile,
        int yTile,
        int scale,
        CancellationToken ctx = default
    )
    {
        var retVal = new TileBlock( this, scale, xTile, yTile );
        await LoadImageAsync( retVal, ctx );

        return retVal;
    }

#pragma warning disable CS1998
    public override async Task<bool> AuthenticateAsync( CancellationToken ctx = default )
#pragma warning restore CS1998
    {
        return !string.IsNullOrEmpty( Name );
    }

    protected override async Task<bool> LoadBlocksInternalAsync(
        IEnumerable<MapBlock> blocks,
        CancellationToken ctx = default
    )
    {
        foreach( var block in blocks )
        {
            await LoadImageAsync( block, ctx );
        }

        return true;
    }

    public override async Task<bool> LoadImageAsync( MapBlock mapBlock, CancellationToken ctx = default )
    {
        var cacheLevel = -1;

        if( TileCaching.Caches.Any() )
            cacheLevel = await TileCaching.LoadImageAsync( mapBlock, ctx );

        if( cacheLevel < 0 )
        {
            mapBlock.ImageData = await GetImageAsync( mapBlock, ctx );

            // indicate that we have to update all caches
            cacheLevel = int.MaxValue;
        }

        if( TileCaching.Caches.Any() )
            await TileCaching.UpdateCaches( mapBlock, cacheLevel, ctx );

        return mapBlock.ImageData != null;
    }

    public override async Task<IMapRegion?> LoadRegionAsync(
        Region region,
        CancellationToken ctx = default( CancellationToken )
    )
    {
        var retVal = DefineBlocksAndOffset( region );
        if( retVal == null )
            return null;

        retVal.ImagesLoaded = await LoadBlocksAsync( retVal.Blocks.Select( b => b.MapBlock ), ctx );

        OnRegionProcessed( retVal.ImagesLoaded );

        return retVal;
    }

    private TiledMapRegion? DefineBlocksAndOffset(Region region)
    {
        var area = region.Area;
        if (area == null)
            return null;

        var heightWidth = GetHeightWidth(region.Scale);
        var projRectangle = new Rectangle2D(heightWidth, heightWidth, coordinateSystem: CoordinateSystem2D.Display);

        var shrinkResult = projRectangle.ShrinkToFit(area, region.ShrinkStyle);

        var tilesHighWide = GetNumTiles(region.Scale);

        var top = area.Min(c => c.Y);
        var firstRow = (int)Math.Floor(top / TileHeightWidth);
        var lastRow = (int)Math.Ceiling(area.Max(c => c.Y) / TileHeightWidth) - 1;

        var left = area.Min(c => c.X);
        var firstCol = (int)Math.Floor(left / TileHeightWidth);
        var lastCol = (int)Math.Ceiling(area.Max(c => c.X) / TileHeightWidth) - 1;

        var centerCol = (int) Math.Floor( region.CenterPoint!.X / TileHeightWidth );

        var excessColumns = lastCol - firstCol + 1 - tilesHighWide;
        if (excessColumns > 0)
        {
            var halfExcessColumns = (int)Math.Floor(excessColumns / 2f);
            firstCol += halfExcessColumns;
            lastCol -= halfExcessColumns;
        }

        var retVal = new TiledMapRegion { Zoom = shrinkResult.Zoom };

        for (var row = firstRow; row <= lastRow; row++)
        {
            if (row < 0 || row >= tilesHighWide)
                continue;

            for (var regionCol = firstCol; regionCol <= lastCol; regionCol++)
            {
                var absoluteCol = regionCol < 0 ? regionCol +tilesHighWide : regionCol;

                //if (regionCol < 0)
                //{
                //    absoluteCol = regionCol + tilesHighWide;
                //    if (absoluteCol < 0 || absoluteCol <= centerCol)
                //        continue;
                //}
                //else
                //{
                //    if (regionCol >= tilesHighWide)
                //    {
                //        absoluteCol = regionCol - tilesHighWide;
                //        if (absoluteCol >= tilesHighWide || absoluteCol >= centerCol)
                //            continue;
                //    }
                //}

                retVal.Blocks.Add(new PositionedMapBlock(row,
                                                           regionCol,
                                                           new TileBlock(this,
                                                                          region.Scale,
                                                                          absoluteCol,
                                                                          row)));
            }
        }

        var topFirstIncludedRow = retVal.Blocks.MinBy(b => b.RegionRow)!.RegionRow * TileHeightWidth;
        var leftFirstIncludedCol = retVal.Blocks.MinBy(b => b.RegionColumn)!.RegionColumn * TileHeightWidth;

        retVal.Offset = new Vector3(leftFirstIncludedCol - left, topFirstIncludedRow - top, 0);

        return retVal;
    }
}
