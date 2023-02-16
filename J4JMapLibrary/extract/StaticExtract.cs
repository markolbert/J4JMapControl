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

using J4JSoftware.Logging;

namespace J4JSoftware.J4JMapLibrary;

public class StaticExtract : MapExtract
{
    public StaticExtract(
        IProjection projection,
        IJ4JLogger logger
    )
        : base( projection, logger )
    {
    }

    public bool TryGetBounds( out StaticBounds? bounds )
    {
        bounds = null;

        if( Tiles.Count == 0 )
        {
            Logger.Error( "No tiles in the extract" );
            return false;
        }

        if( Tiles.Count > 1 )
        {
            Logger.Error( "More than one mapFragment in the extract" );
            return false;
        }

        var castTile = Tiles[ 0 ] as IStaticFragment;
        if( castTile == null )
        {
            Logger.Error( "Tile is not a IStaticFragment" );
            return false;
        }

        bounds = new StaticBounds( castTile );
        return true;
    }
}
