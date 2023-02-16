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

using J4JSoftware.Logging;

namespace J4JSoftware.J4JMapLibrary;

[ Projection( "OpenTopoMaps" ) ]
public sealed class OpenTopoMapsProjection : OpenMapProjection
{
    public OpenTopoMapsProjection(
        IJ4JLogger logger,
        ITileCache? cache = null
    )
        : base( logger, cache )
    {
        MapServer = new OpenTopoMapServer();
        TiledScale = new TiledScale( MapServer );
    }

    public OpenTopoMapsProjection(
        IProjectionCredentials credentials,
        IJ4JLogger logger,
        ITileCache? cache = null
    )
        : base( credentials, logger, cache )
    {
        MapServer = new OpenTopoMapServer();
        TiledScale = new TiledScale( MapServer );
    }

    public override IMapServer MapServer { get; }
}
