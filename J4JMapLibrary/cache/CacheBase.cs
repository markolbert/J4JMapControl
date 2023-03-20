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

public abstract class CacheBase : ITileCache
{
    protected CacheBase(
        ILogger logger
    )
    {
        Logger = logger;
        Logger.ForContext( GetType() );
    }

    protected ILogger Logger { get; }

    public CacheStats Stats { get; } = new();

    public int MaxEntries { get; set; }
    public long MaxBytes { get; set; }
    public TimeSpan RetentionPeriod { get; set; } = TimeSpan.Zero;

    public ITileCache? ParentCache { get; set; }

    public abstract void Clear();
    public abstract void PurgeExpired();

    public async Task<byte[]?> GetImageDataAsync( ITiledFragment fragment, CancellationToken ctx = default )
    {
        var retVal = await GetImageDataInternalAsync( fragment, ctx );
        if( retVal != null )
            return retVal;

        retVal = ParentCache == null
            ? null
            : await ParentCache.GetImageDataAsync(fragment, ctx);

        retVal ??= await AddEntryAsync(fragment, ctx);

        if( retVal != null )
            return retVal;

        Logger.Error("Failed to create {0} cache entry for mapFragment ({1}, {2})",
                     fragment.Projection.Name,
                     fragment.MapXTile,
                     fragment.MapYTile);

        return null;
    }

    protected abstract Task<byte[]?> GetImageDataInternalAsync(
        ITiledFragment fragment,
        CancellationToken ctx = default
    );

    protected abstract Task<byte[]?> AddEntryAsync(
        ITiledFragment fragment,
        CancellationToken ctx = default
    );

    public abstract IEnumerator<CachedEntry> GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
