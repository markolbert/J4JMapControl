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

using System.Runtime.CompilerServices;
using J4JSoftware.Logging;

namespace J4JSoftware.J4JMapLibrary;

public abstract class StaticProjection<TAuth> : Projection<TAuth, INormalizedViewport, IStaticFragment>
    where TAuth : class, new()
{
    protected StaticProjection(
        IJ4JLogger logger
    )
        : base( logger )
    {
    }

    public override async IAsyncEnumerable<StaticFragment> GetExtractAsync(
        INormalizedViewport viewportData,
        [ EnumeratorCancellation ] CancellationToken ctx = default
    )
    {
        if( !Initialized )
        {
            Logger.Error( "Projection not initialized" );
            yield break;
        }

        var mapTile = new StaticFragment( this, viewportData );
        mapTile.ImageData = await mapTile.GetImageAsync(ctx);

        yield return mapTile;
    }

    public override IMapFragment? GetFragment( int xTile, int yTile, int scale ) =>
        Task.Run( async () => await GetFragmentAsync( xTile, yTile, scale ) ).Result;

    public override async Task<IMapFragment?> GetFragmentAsync(
        int xTile,
        int yTile,
        int scale,
        CancellationToken ctx = default
    )
    {
        xTile = GetTileXRange( scale ).ConformValueToRange( xTile, "GetFragmentAsync xTile" );
        yTile = GetTileYRange( scale ).ConformValueToRange( yTile, "GetFragmentAsync yTile" );

        var centerX = (int) Math.Round( ( xTile + 0.5F ) * TileHeightWidth );
        var centerY = (int) Math.Round( ( yTile + 0.5F ) * TileHeightWidth );

        var centerPoint = new StaticPoint( this );
        centerPoint.SetCartesian( centerX, centerY );

        var retVal = new StaticFragment( this,
                                         centerPoint.Latitude,
                                         centerPoint.Longitude,
                                         scale,
                                         TileHeightWidth,
                                         TileHeightWidth );

        retVal.ImageData = await retVal.GetImageAsync( ctx );

        return retVal;
    }
}
