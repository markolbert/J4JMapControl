﻿// Copyright (c) 2021, 2022 Mark A. Olbert 
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

public interface IProjection
{
    string Name { get; }

    bool Initialized { get; }

    IMapServer MapServer { get; }
    IProjectionScale MapScale { get; }

    Task<bool> AuthenticateAsync( object? credentials, CancellationToken ctx = default );

    IAsyncEnumerable<IMapFragment> GetExtractAsync(
        INormalizedViewport viewportData,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    );
}

public interface IProjection<in TAuth, in TViewport, out TFrag> : IProjection
    where TAuth : class
    where TViewport : INormalizedViewport
    where TFrag : IMapFragment
{
    Task<bool> AuthenticateAsync( TAuth? credentials, CancellationToken ctx = default );

    IAsyncEnumerable<TFrag> GetExtractAsync(
        TViewport viewportData,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    );
}
