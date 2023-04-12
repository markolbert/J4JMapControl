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

using J4JSoftware.J4JMapLibrary.MapRegion;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapLibrary;

public abstract class TiledProjection<TAuth> : Projection<TAuth>, ITiledProjection
    where TAuth : class, new()
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

    public override async Task<MapTile> GetMapTileWraparoundAsync(
        int xTile,
        int yTile,
        int scale,
        CancellationToken ctx = default
    )
    {
        var region = new MapRegion.MapRegion( this, LoggerFactory ) { Scale = scale };

        var retVal = new MapTile( region, yTile ).SetXRelative( xTile );
        await LoadImageAsync( retVal, ctx );

        return retVal;
    }

    public override async Task<MapTile> GetMapTileAbsoluteAsync(
        int xTile,
        int yTile,
        int scale,
        CancellationToken ctx = default
    )
    {
        var region = new MapRegion.MapRegion( this, LoggerFactory ) { Scale = scale };

        var retVal = new MapTile( region, yTile ).SetXAbsolute( xTile );
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

        var cacheLevel = -1;

        if( TileCaching.Caches.Any() )
            cacheLevel = await TileCaching.LoadImageAsync( mapTile, ctx );

        if( cacheLevel < 0 )
        {
            mapTile.ImageData = await GetImageAsync( mapTile, ctx );

            // indicate that we have to update all caches
            cacheLevel = int.MaxValue;
        }

        if( TileCaching.Caches.Any() )
            await TileCaching.UpdateCaches( mapTile, cacheLevel, ctx );

        return mapTile.ImageData != null;
    }
}
