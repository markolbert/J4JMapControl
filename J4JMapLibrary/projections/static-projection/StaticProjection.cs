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

public abstract class StaticProjection<TAuth> : Projection<TAuth, INormalizedViewport, StaticFragment>
    where TAuth : class
{
    protected StaticProjection(
        IJ4JLogger logger
    )
        : base( logger )
    {
    }

    protected StaticProjection(
        IProjectionCredentials credentials,
        IJ4JLogger logger
    )
        : base( credentials, logger )
    {
    }

    public override async IAsyncEnumerable<StaticFragment> GetExtractAsync(
        INormalizedViewport viewportData,
        bool deferImageLoad = false,
        [ EnumeratorCancellation ] CancellationToken ctx = default
    )
    {
        if( !Initialized )
        {
            Logger.Error( "Projection not initialized" );
            yield break;
        }

        var mapTile = new StaticFragment( MapServer, viewportData );

        if( !deferImageLoad )
            await mapTile.GetImageAsync( ctx: ctx );

        yield return mapTile;
    }
}
