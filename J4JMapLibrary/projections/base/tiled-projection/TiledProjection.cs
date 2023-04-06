﻿// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
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

using J4JSoftware.J4JMapLibrary.MapRegion;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapLibrary;

public abstract class TiledProjection<TAuth> : Projection<TAuth>, ITiledProjection
    where TAuth : class, new()
{
    protected TiledProjection(
        ILoggerFactory? loggerFactory
    )
        : base( loggerFactory )
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
            Logger?.LogError("Projection is not initialized");
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

    public override async Task<MapTile> GetMapTileByProjectionCoordinatesAsync(
        int x,
        int y,
        int scale,
        CancellationToken ctx = default
    )
    {
        var region = new MapRegion.MapRegion( this, LoggerFactory ) { Scale = scale };

        var retVal = new MapTile( region, y ).SetXRelative( x );
        await LoadImageAsync( retVal, ctx );

        return retVal;
    }

    public override async Task<MapTile> GetMapTileByRegionCoordinatesAsync(
        int x,
        int y,
        int scale,
        CancellationToken ctx = default
    )
    {
        var region = new MapRegion.MapRegion( this, LoggerFactory ) { Scale = scale };

        var retVal = new MapTile( region, y ).SetXAbsolute( x );
        await LoadImageAsync(retVal, ctx);

        return retVal;
    }

#pragma warning disable CS1998
    protected override async Task<bool> AuthenticateAsync( CancellationToken ctx = default )
#pragma warning restore CS1998
    {
        return !string.IsNullOrEmpty( Name );
    }

    protected override async Task<bool> LoadRegionInternalAsync(
        MapRegion.MapRegion region,
        CancellationToken ctx = default
    )
    {
        foreach( var mapTile in region )
        {
            await LoadImageAsync( mapTile, ctx );
        }

        return true;
    }

    public override async Task<bool> LoadImageAsync(MapTile mapTile, CancellationToken ctx = default)
    {
        if (!mapTile.InProjection)
        {
            mapTile.ImageData = null;
            return true;
        }

        var retVal = false;

        if( TileCache != null )
            retVal = await TileCache.LoadImageAsync( mapTile, ctx );

        if (retVal)
            return true;

        mapTile.ImageData = await GetImageAsync( mapTile, ctx );
        retVal = mapTile.ImageData != null;

        if( retVal && TileCache != null )
            await TileCache.AddEntryAsync( mapTile, ctx );

        return retVal;
    }
}
