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

public interface IMapFragment
{
    event EventHandler? ImageChanged;

    public string FragmentId { get; }

    IMapServer MapServer { get; }

    int X { get; }
    int Y { get; }
    float ActualHeight { get; }
    float ActualWidth { get; }

    public byte[]? ImageData { get; }
    long ImageBytes { get; }

    byte[]? GetImage( bool forceRetrieval = false );
    Task<byte[]?> GetImageAsync( bool forceRetrieval = false, CancellationToken ctx = default );
}
