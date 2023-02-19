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

namespace J4JSoftware.J4JMapLibrary;

public interface IMapServer
{
    event EventHandler? ScaleChanged;

    string SupportedProjection { get; }
    bool Initialized { get; }

    int Scale { get; set; }
    int MinScale { get; }
    int MaxScale { get; }
    MinMax<int> ScaleRange { get; }

    float MaxLatitude { get; }
    float MinLatitude { get; }
    MinMax<float> LatitudeRange { get; }

    float MaxLongitude { get; }
    float MinLongitude { get; }
    MinMax<float> LongitudeRange { get; }

    MinMax<int> XRange { get; }
    MinMax<int> YRange { get; }

    int MaxRequestLatency { get; set; }
    int TileHeightWidth { get; }
    string ImageFileExtension { get; }

    string Copyright { get; }
    Uri? CopyrightUri { get; }

    HttpRequestMessage? CreateMessage( object requestInfo, int scale );
}

public interface IMapServer<in TTile, in TAuth> : IMapServer
    where TTile : class
    where TAuth : class
{
    Task<bool> InitializeAsync( TAuth credentials, CancellationToken ctx = default );
    HttpRequestMessage? CreateMessage( TTile tile, int scale );
}
