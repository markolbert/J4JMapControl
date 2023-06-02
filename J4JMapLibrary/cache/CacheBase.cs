#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// CacheBase.cs
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

using System.Collections;
using J4JSoftware.J4JMapLibrary.MapRegion;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapLibrary;

public abstract class CacheBase : ITileCache
{
    protected CacheBase(
        string name,
        ILoggerFactory? loggerFactory
    )
    {
        Name = name;
        Stats = new CacheStats( name );
        Logger = loggerFactory?.CreateLogger( GetType() );
    }

    protected ILogger? Logger { get; }

    public string Name { get; }

    public int MaxEntries { get; set; }
    public long MaxBytes { get; set; }
    public TimeSpan RetentionPeriod { get; set; } = TimeSpan.Zero;

    public CacheStats Stats { get; }

    public abstract void Clear();
    public abstract void PurgeExpired();

    public async Task<bool> LoadImageAsync( MapBlock mapBlock, CancellationToken ctx = default )
    {
        if( await LoadImageDataInternalAsync( mapBlock, ctx ) )
            return true;

        Logger?.LogTrace( "{0} Failed to find {1} cache entry for mapFragment ({2}, {3})",
                          GetType(),
                          mapBlock.Projection.Name,
                          mapBlock.X,
                          mapBlock.Y );

        return false;
    }

    public abstract Task<bool> AddEntryAsync( MapBlock mapBlock, CancellationToken ctx = default );

    public abstract IEnumerator<CachedEntry> GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected abstract Task<bool> LoadImageDataInternalAsync( MapBlock mapBlock, CancellationToken ctx = default );
}
