﻿#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// FileSystemCache.cs
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

using J4JSoftware.J4JMapLibrary.MapRegion;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapLibrary;

// it's presumed that the cache directory contains NOTHING BUT TILE IMAGE FILES
public class FileSystemCache : CacheBase
{
    private string? _cacheDir;

    public FileSystemCache(
        string name,
        ILoggerFactory? loggerFactory = null
    )
        : base( name, loggerFactory )
    {
    }

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
                Stats.Reload(GetFiles());
            }
            catch
            {
                Logger?.LogError("Cache path '{0}' is not accessible", value);
            }
        }
    }

    public override void Clear()
    {
        if( string.IsNullOrEmpty( _cacheDir ) )
        {
            Logger?.LogError("Caching directory is undefined");
            return;
        }

        foreach( var fileInfo in GetFiles() )
        {
            File.Delete( fileInfo.FullName );
        }

        Stats.Reload( GetFiles() );
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

        return retVal;
    }

    public override void PurgeExpired()
    {
        if( string.IsNullOrEmpty( _cacheDir ) )
        {
            Logger?.LogError("Caching directory is undefined");
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
            files = files.OrderBy( x => x.CreationTime )
                         .ToList();

            while( files.Count > MaxEntries )
            {
                File.Delete( files.First().FullName );
                files.RemoveAt( 0 );
            }
        }

        if( MaxBytes <= 0 || files.Sum( x => x.Length ) <= MaxBytes )
        {
            Stats.Reload( files );
            return;
        }

        files = files.OrderByDescending( x => x.Length )
                     .ToList();

        while( MaxBytes >= 0 && files.Sum( x => x.Length ) > MaxBytes )
        {
            File.Delete( files.First().FullName );
            files.RemoveAt( 0 );
        }

        Stats.Reload( files );
    }

    protected override async Task<bool> LoadImageDataInternalAsync( MapTile mapTile, CancellationToken ctx = default )
    {
        if( string.IsNullOrEmpty( _cacheDir ) )
        {
            Logger?.LogError("Caching directory is undefined");
            return false;
        }

        var filePath = Path.Combine( _cacheDir, $"{mapTile.FragmentId}{mapTile.Region.Projection.ImageFileExtension}" );

        if( !File.Exists( filePath ) )
            return false;

        mapTile.ImageData = await File.ReadAllBytesAsync( filePath, ctx );

        return mapTile.ImageBytes > 0;
    }

    public override async Task<bool> AddEntryAsync( MapTile mapTile, CancellationToken ctx = default )
    {
        if( string.IsNullOrEmpty( _cacheDir ) )
        {
            Logger?.LogError("Caching directory is undefined");
            return false;
        }

        if( mapTile.ImageData == null )
        {
            Logger?.LogError("Map tile contains no image data");
            return false;
        }

        var fileName = $"{mapTile.FragmentId}{mapTile.Region.Projection.ImageFileExtension}";
        var filePath = Path.Combine( _cacheDir, fileName );

        if( File.Exists( filePath ) )
            Logger?.LogWarning("Replacing file system cache entry '{0}'", mapTile.FragmentId);
        else Logger?.LogTrace("Added file system cache entry '{fragmentId}'", mapTile.FragmentId);

        try
        {
            await using var imgFile = File.Create( filePath );
            await imgFile.WriteAsync( mapTile.ImageData, ctx );
            imgFile.Close();

            Stats.RecordEntry( mapTile.ImageData );
        }
        catch( Exception ex )
        {
            Logger?.LogError( "Problem updating {cache} re: image file '{file}', message was {mesg}",
                              typeof( FileSystemCache ),
                              filePath,
                              ex.Message );
        }

        if( ( MaxEntries > 0 && Stats.Entries > MaxEntries ) || ( MaxBytes > 0 && Stats.Bytes > MaxBytes ) )
            PurgeExpired();

        return true;
    }

    public override IEnumerator<CachedEntry> GetEnumerator()
    {
        if( string.IsNullOrEmpty( _cacheDir ) )
        {
            Logger?.LogError("Caching directory is undefined");
            yield break;
        }

        foreach( var path in Directory.EnumerateFiles( _cacheDir,
                                                       "*",
                                                       new EnumerationOptions
                                                       {
                                                           IgnoreInaccessible = true, RecurseSubdirectories = false
                                                       } ) )
        {
            var fileName = Path.GetFileNameWithoutExtension( path );
            var parts = fileName.Split( '-' );

            if( parts.Length != 2 )
            {
                Logger?.LogError("Unable to parse cached filename '{0}'", fileName);
                continue;
            }

            if( !MapExtensions.TryParseQuadKey( parts[ 1 ], out _ ) )
            {
                Logger?.LogError("Could not deconstruct quadkey for file '{0}'", fileName);
                continue;
            }

            var fileInfo = new FileInfo( path );

            yield return new CachedEntry( fileInfo.Length, fileInfo.CreationTimeUtc, fileInfo.LastAccessTime );
        }
    }
}
