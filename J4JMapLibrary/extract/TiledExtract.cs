// Copyright (c) 2021, 2022 Mark A. Olbert 
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

public class TiledExtract : MapExtract
{
    public TiledExtract(
        ITiledProjection projection,
        IJ4JLogger logger
    )
        : base( projection, logger )
    {
    }

    public bool TryGetBounds( out TiledBounds? bounds )
    {
        bounds = null;

        var castTiles = Tiles.Cast<ITiledFragment>().ToList();

        if( castTiles.Count == 0 )
        {
            Logger.Error( Tiles.Any()
                              ? "MapExtract contains tiles that aren't ITiledFragment"
                              : "No tiles in the extract" );

            return false;
        }

        var minX = castTiles.Min( x => x.X );
        var maxX = castTiles.Max( x => x.X );
        var minY = castTiles.Min( x => x.Y );
        var maxY = castTiles.Max( x => x.Y );

        bounds = new TiledBounds( new TileCoordinates( minX, minY ), new TileCoordinates( maxX, maxY ) );

        return true;
    }
}
