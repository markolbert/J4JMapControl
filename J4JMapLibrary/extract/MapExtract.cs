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

using System.Collections;
using Serilog;

namespace J4JSoftware.J4JMapLibrary;

public abstract class MapExtract : IMapExtract
{
    protected MapExtract(
        IProjection projection,
        ILogger logger
    )
    {
        Projection = projection;

        Logger = logger;
        Logger.ForContext( GetType() );
    }

    protected ILogger Logger { get; }
    protected IProjection Projection { get; }
    protected List<IMapFragment> Tiles { get; } = new();

    public bool Add( IMapFragment mapFragment )
    {
        if( Tiles.Count == 0 )
        {
            Tiles.Add( mapFragment );
            return true;
        }

        if( !Tiles[ 0 ].Projection.Equals(mapFragment.Projection) )
            return false;

        Tiles.Add( mapFragment );
        return true;
    }

    public void Remove( IMapFragment mapFragment ) => Tiles.Remove( mapFragment );

    public void RemoveAt( int idx )
    {
        if( idx >= 0 && idx < Tiles.Count )
            Tiles.RemoveAt( idx );
    }

    public void Clear() => Tiles.Clear();

    public IEnumerator<IMapFragment> GetEnumerator() => ( (IEnumerable<IMapFragment>) Tiles ).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
