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

using System.Collections.ObjectModel;
using J4JSoftware.Logging;

namespace J4JSoftware.J4JMapLibrary;

public class FileSystemCache : CacheBase
{
    private long _bytesCached;

    private string? _cacheDir;
    private int _tilesCached;

    public FileSystemCache(
        IJ4JLogger logger
    )
        : base( logger )
    {
    }

    public override int Count => GetFiles().Count;

    public override ReadOnlyCollection<string> FragmentIds =>
        GetFiles()
           .Select( x =>
            {
                var woExt = Path.GetFileNameWithoutExtension( x.Name );
                var lastDash = woExt.LastIndexOf( "-", StringComparison.InvariantCultureIgnoreCase );
                return lastDash > 0 && lastDash < woExt.Length ? woExt[ ( lastDash + 1 ).. ] : string.Empty;
            } )
           .Where( x => !string.IsNullOrEmpty( x ) )
           .OrderBy( x => x )
           .ToList()
           .AsReadOnly();

    public bool IsValid => _cacheDir != null;

    public string? CacheDirectory
    {
        get => _cacheDir;

        set
        {
            if( value == null )
            {
                _cacheDir = null;
                return;
            }

            try
            {
                var dirInfo = Directory.CreateDirectory( value );
                _cacheDir = dirInfo.FullName;
            }
            catch
            {
                Logger.Error<string>( "Cache path '{0}' is not accessible", value );
            }
        }
    }

    public override void Clear()
    {
        if( string.IsNullOrEmpty( _cacheDir ) )
        {
            Logger.Error( "Caching directory is undefined" );
            return;
        }

        foreach( var fileInfo in GetFiles() )
        {
            File.Delete( fileInfo.FullName );
        }
    }

    // assumes _cacheDir is defined
    private List<FileInfo> GetFiles()
    {
        var retVal = Directory.GetFiles( _cacheDir!,
                                         "*.*",
                                         new EnumerationOptions
                                         {
                                             IgnoreInaccessible = true, RecurseSubdirectories = true
                                         } )
                              .Select( x => new FileInfo( x ) )
                              .ToList();

        _tilesCached = retVal.Count;
        _bytesCached = retVal.Sum( x => x.Length );

        return retVal;
    }

    public override void PurgeExpired()
    {
        if( string.IsNullOrEmpty( _cacheDir ) )
        {
            Logger.Error( "Caching directory is undefined" );
            return;
        }

        var files = GetFiles();
        var deleted = new List<string>();

        foreach( var fileInfo in files.Where( x => RetentionPeriod != TimeSpan.Zero
                                               && x.LastAccessTime < DateTime.Now - RetentionPeriod ) )
        {
            File.Delete( fileInfo.FullName );
            deleted.Add( fileInfo.FullName );
        }

        files = files.Where( x => !deleted.Any( y => y.Equals( x.FullName ) ) )
                     .ToList();

        if( MaxEntries > 0 && files.Count > MaxEntries )
        {
            var filesByOldest = files.OrderBy( x => x.CreationTime )
                                     .ToList();

            while( filesByOldest.Count > MaxEntries )
            {
                File.Delete( filesByOldest.First().FullName );
                filesByOldest.RemoveAt( 0 );
            }
        }

        if( MaxBytes <= 0 || files.Sum( x => x.Length ) <= MaxBytes )
            return;

        var filesByLargest = files.OrderByDescending( x => x.Length )
                                  .ToList();

        while( MaxBytes >= 0 && filesByLargest.Sum( x => x.Length ) > MaxBytes )
        {
            File.Delete( filesByLargest.First().FullName );
            filesByLargest.RemoveAt( 0 );
        }
    }

    protected override async Task<CacheEntry?> GetEntryInternalAsync(
        ITiledProjection projection,
        int xTile,
        int yTile,
        int scale,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    )
    {
        if( string.IsNullOrEmpty( _cacheDir ) )
        {
            Logger.Error( "Caching directory is undefined" );
            return null;
        }

        var key = $"{projection.Name}-{projection.GetQuadKey( xTile, yTile, scale )}";
        var filePath = Path.Combine( _cacheDir, $"{key}{projection.MapServer.ImageFileExtension}" );

        return File.Exists( filePath )
            ? new CacheEntry( projection, xTile, yTile, scale, await File.ReadAllBytesAsync( filePath, ctx ) )
            : deferImageLoad
                ? new CacheEntry( projection, xTile, yTile, scale, ctx )
                : null;
    }

    protected override async Task<CacheEntry?> AddEntryAsync(
        ITiledProjection projection,
        int xTile,
        int yTile,
        int scale,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    )
    {
        if( string.IsNullOrEmpty( _cacheDir ) )
        {
            Logger.Error( "Caching directory is undefined" );
            return null;
        }

        var retVal = new CacheEntry( projection, xTile, yTile, scale, ctx );

        var fileName = $"{projection.Name}-{retVal.Tile.FragmentId}{projection.MapServer.ImageFileExtension}";
        var filePath = Path.Combine( _cacheDir, fileName );

        var bytesToWrite = retVal.Tile.ImageBytes <= 0L
            ? deferImageLoad ? null : await retVal.Tile.GetImageAsync( ctx: ctx ) ?? null
            : await retVal.Tile.GetImageAsync( ctx: ctx ) ?? null;

        if( bytesToWrite == null )
        {
            if( !deferImageLoad )
                Logger.Error( "Failed to retrieve image data" );

            return null;
        }

        if( File.Exists( filePath ) )
            Logger.Warning<string>( "Replacing map mapFragment with fragment ID '{0}'", retVal.Tile.FragmentId );

        await using var imgFile = File.Create( filePath );
        await imgFile.WriteAsync( bytesToWrite, ctx );
        imgFile.Close();

        _tilesCached++;
        _bytesCached += bytesToWrite.Length;

        if( ( MaxEntries > 0 && _tilesCached > MaxEntries )
        || ( MaxBytes > 0 && _bytesCached > MaxBytes ) )
            PurgeExpired();

        return retVal;
    }
}
