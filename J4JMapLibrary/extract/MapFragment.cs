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

using J4JSoftware.DeusEx;
using Serilog;

namespace J4JSoftware.J4JMapLibrary;

public class MapFragment : IMapFragment
{
    public event EventHandler? ImageChanged;

    protected MapFragment(
        IProjection projection
    )
    {
        Logger = J4JDeusEx.GetLogger();
        Logger?.ForContext( GetType() );

        Projection = projection;
    }

    protected ILogger? Logger { get; }

    public IProjection Projection { get; }
    
    public float ImageHeight { get; set; }
    public float ImageWidth { get; set; }

    public bool InViewport { get; set; }

    public string FragmentId { get; init; } = string.Empty;

    // XTile and YTile can be any integer values
    public int XTile { get; init; }
    public int YTile { get; init; }

    // MapXTile and MapYTile, in tiled projections, must be constrained to 
    // the range 0..(number of tiles at Scale) - 1
    public int MapXTile { get; init; }
    public int MapYTile { get; init; }

    public int Scale { get; protected set; }

    public byte[]? ImageData { get; set; }
    public long ImageBytes => ImageData?.Length <= 0 ? -1 : ImageData?.Length ?? -1;
}
