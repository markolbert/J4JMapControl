#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// IProjection.cs
//
// This file is part of JumpForJoy Software's J4JMapLibrary.
// 
// J4JMapLibrary is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JMapLibrary is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JMapLibrary. If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.Collections.ObjectModel;

namespace J4JSoftware.J4JMapLibrary;

public interface IProjection : IEquatable<IProjection>
{
    public event EventHandler<bool>? RegionProcessed;

    string Name { get; }
    bool Initialized { get; }
    int MaxRequestLatency { get; set; }
    string ImageFileExtension { get; }
    string Copyright { get; }
    Uri? CopyrightUri { get; }

    float MaxLatitude { get; }
    float MinLatitude { get; }
    MinMax<float> LatitudeRange { get; }

    float MaxLongitude { get; }
    float MinLongitude { get; }
    MinMax<float> LongitudeRange { get; }

    int MinScale { get; }
    int MaxScale { get; }
    MinMax<int> ScaleRange { get; }

    MinMax<float> GetXYRange(int scale);
    MinMax<int> GetTileRange(int scale);

    int TileHeightWidth { get; }
    int GetHeightWidth(int scale);
    int GetNumTiles(int scale);

    bool SupportsStyles { get; }
    string? MapStyle { get; set; }
    ReadOnlyCollection<string> MapStyles { get; }
    bool IsStyleSupported( string style );

    bool SetCredentials( object credentials );
    bool Authenticate();
    Task<bool> AuthenticateAsync( CancellationToken ctx = default );

    float? GetRegionZoom(Region requestedRegion);

    Task<MapBlock?> GetMapTileAsync( int x, int y, int scale, CancellationToken ctx = default );
    Task<byte[]?> GetImageAsync(MapBlock mapBlock, CancellationToken ctx = default);
    Task<bool> LoadImageAsync(MapBlock mapBlock, CancellationToken ctx = default);

    Task<IMapRegion?> LoadRegionAsync(
        Region region,
        CancellationToken ctx = default(CancellationToken)
    );

    Task<bool> LoadBlocksAsync(
        IEnumerable<MapBlock> blocks,
        CancellationToken ctx = default
    );
}
