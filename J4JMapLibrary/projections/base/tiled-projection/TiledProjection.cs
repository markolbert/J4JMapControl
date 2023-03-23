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
using Serilog;

namespace J4JSoftware.J4JMapLibrary;

public abstract class TiledProjection<TAuth> : Projection<TAuth>, ITiledProjection
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
    protected override async Task<bool> AuthenticateAsync( CancellationToken ctx = default )
#pragma warning restore CS1998
    {
        return !string.IsNullOrEmpty( Name );
    }

    public override async Task<MapTile> GetMapTileAsync( int x, int y, int scale, CancellationToken ctx = default )
    {
        var retVal = new MapTile( new MapRegion.MapRegion( this, Logger ) { Scale = scale }, x, y );
        await retVal.LoadFromCacheAsync( TileCache, ctx );

        return retVal;
    }

    protected override async Task<bool> LoadRegionInternalAsync(MapRegion.MapRegion region, CancellationToken ctx = default )
    {
        foreach( var mapTile in region )
        {
            if( mapTile.InProjection )
                await mapTile.LoadFromCacheAsync( TileCache, ctx );
        }

        return true;
    }
}
