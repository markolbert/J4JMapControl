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

using J4JSoftware.J4JMapLibrary.MapRegion;

namespace J4JSoftware.J4JMapLibrary;

public interface IProjection : IEquatable<IProjection>
{
    string Name { get; }

    bool Initialized { get; }

    int MinScale { get; }
    int MaxScale { get; }
    MinMax<int> ScaleRange { get; }

    float MaxLatitude { get; }
    float MinLatitude { get; }
    MinMax<float> LatitudeRange { get; }

    float MaxLongitude { get; }
    float MinLongitude { get; }
    MinMax<float> LongitudeRange { get; }

    int MaxRequestLatency { get; set; }
    int TileHeightWidth { get; }
    string ImageFileExtension { get; }

    string Copyright { get; }
    Uri? CopyrightUri { get; }

    string MapStyle { get; set; }
    event EventHandler<bool>? LoadComplete;

    int GetHeightWidth( int scale );

    MinMax<float> GetXYRange( int scale );
    MinMax<int> GetTileRange( int scale );
    int GetNumTiles( int scale );

    HttpRequestMessage? CreateMessage( MapTile mapTile );

    bool SetCredentials( object credentials );
    Task<bool> SetCredentialsAsync( object credentials, CancellationToken ctx = default );

    Task<MapTile> GetMapTileByProjectionCoordinatesAsync( int x, int y, int scale, CancellationToken ctx = default );
    Task<MapTile> GetMapTileByRegionCoordinatesAsync( int x, int y, int scale, CancellationToken ctx = default );

    Task<bool> LoadRegionAsync(
        MapRegion.MapRegion region,
        CancellationToken ctx = default
    );

    byte[]? GetImage( MapTile mapTile );
    Task<byte[]?> GetImageAsync( MapTile mapTile, CancellationToken ctx = default );

    Task<bool> LoadImageAsync( MapTile mapTile, CancellationToken ctx = default );
}
