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

using J4JSoftware.DeusEx;
using J4JSoftware.Logging;

namespace J4JSoftware.J4JMapLibrary;

public abstract class MapFragment : IMapFragment
{
    public event EventHandler? ImageChanged;

    protected MapFragment(
        IProjection projection
    )
    {
        Logger = J4JDeusEx.GetLogger();
        Logger?.SetLoggedType( GetType() );

        Projection = projection;
    }

    protected IJ4JLogger? Logger { get; }

    public IProjection Projection { get; }
    
    public float ImageHeight { get; set; }
    public float ImageWidth { get; set; }
    public bool InViewport { get; set; }

    public string FragmentId { get; init; } = string.Empty;

    public int XTile { get; init; }
    public int YTile { get; init; }
    public int Scale { get; protected set; }

    public byte[]? ImageData { get; set; }
    public long ImageBytes => ImageData?.Length <= 0 ? -1 : ImageData?.Length ?? -1;
}
