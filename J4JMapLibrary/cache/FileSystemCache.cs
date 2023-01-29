﻿using J4JSoftware.Logging;

namespace J4JMapLibrary;

public class FileSystemCache : CacheBase
{
    private readonly FileLocator _fileLocator;

    private string? _cacheDir;

    public FileSystemCache( 
        IJ4JLogger logger
    )
        : base( logger )
    {
    }

    public int MaxEntries { get; set; }
    public TimeSpan RetentionPeriod { get; set; } = TimeSpan.Zero;

    public string? CacheDirectory
    {
        get => _cacheDir;

        set
        {

        }
    }

    public override void Clear() => _cached.Clear();

    public override void PurgeExpired()
    {
        if( RetentionPeriod == TimeSpan.Zero ) 
            return;

        var toDelete = _cached.Where( x => x.Value.LastAccessedUtc < DateTime.UtcNow - RetentionPeriod )
                              .Select( x => x.Key )
                              .ToList();

        foreach( var key in toDelete )
        {
            _cached.Remove( key );
        }
    }

    protected override CacheEntry? GetEntryInternal( ITiledProjection projection, int xTile, int yTile )
    {
        var key = $"{projection.Name}{projection.GetQuadKey( xTile, yTile )}";

        return _cached.ContainsKey(key) ? _cached[key] : null;
    }

    protected override CacheEntry? AddEntry( ITiledProjection projection, int xTile, int yTile )
    {
        var retVal = new CacheEntry( projection, xTile, yTile );

        var key = $"{projection.Name}{retVal.Tile.QuadKey}";

        if( _cached.ContainsKey( key ) )
        {
            Logger.Warning<string>( "Replacing map tile with quadkey '{0}'", retVal.Tile.QuadKey );
            _cached[ key ] = retVal;
        }
        else
        {
            _cached.Add( key, retVal );

            if( MaxEntries > 0 && _cached.Count > MaxEntries )
                PurgeExpired();
        }

        return retVal;
    }
}
