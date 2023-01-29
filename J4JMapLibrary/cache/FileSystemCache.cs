using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;

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
        _fileLocator = new FileLocator().StopOnFirstMatch().Readable().Writeable();
    }

    public int MaxBytes { get; set; }
    public TimeSpan RetentionPeriod { get; set; } = TimeSpan.Zero;

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

            _fileLocator.FileToFind( new Guid().ToString() );
            _fileLocator.ScanDirectory( value );

            if( _fileLocator.Matches == 0 )
                Logger.Error<string>( "Cache path '{0}' is not accessible", value );
            else _cacheDir = value;
        }
    }

    public override void Clear()
    {
        if( string.IsNullOrEmpty( _cacheDir ) )
        {
            Logger.Error("Caching directory is undefined");
            return;
        }

        foreach( var fileInfo in GetFiles() )
        {
            File.Delete( fileInfo.FullName );
        }
    }

    // assumes _cacheDir is defined
    private List<FileInfo> GetFiles() =>
        Directory.GetFiles( _cacheDir!,
                            "*.*",
                            enumerationOptions: new EnumerationOptions()
                            {
                                IgnoreInaccessible = true, RecurseSubdirectories = true
                            } )
                 .Select( x => new FileInfo( x ) )
                 .ToList();

    public override void PurgeExpired()
    {
        if( string.IsNullOrEmpty( _cacheDir ) )
        {
            Logger.Error("Caching directory is undefined"  );
            return;
        }

        if( RetentionPeriod == TimeSpan.Zero ) 
            return;

        var files = GetFiles();
        var deleted = new List<string>();

        foreach( var fileInfo in files.Where(x=> x.LastAccessTime < DateTime.UtcNow - RetentionPeriod  ) )
        {
            File.Delete(fileInfo.FullName);
            deleted.Add( fileInfo.FullName );
        }

        files = files.Where( x => !deleted.Any( y => y.Equals( x.FullName ) ) )
                     .OrderByDescending( x => x.Length )
                     .ToList();

        while( MaxBytes >= 0 && files.Sum( x => x.Length ) > MaxBytes )
        {
            File.Delete(files.First().FullName  );
            files.RemoveAt( 0 );
        }
    }

    protected override async Task<CacheEntry?> GetEntryInternalAsync( ITiledProjection projection, int xTile, int yTile )
    {
        if( string.IsNullOrEmpty( _cacheDir ) )
        {
            Logger.Error( "Caching directory is undefined" );
            return null;
        }

        var key = $"{projection.Name}{projection.GetQuadKey( xTile, yTile )}";
        var filePath = Path.Combine( _cacheDir, key );

        return File.Exists( filePath )
            ? new CacheEntry( projection, xTile, yTile, await File.ReadAllBytesAsync( filePath ) )
            : null;
    }

    protected override async Task<CacheEntry?> AddEntryAsync( ITiledProjection projection, int xTile, int yTile )
    {
        if (string.IsNullOrEmpty(_cacheDir))
        {
            Logger.Error("Caching directory is undefined");
            return null;
        }

        var retVal = new CacheEntry( projection, xTile, yTile );

        var key = $"{projection.Name}{retVal.Tile.QuadKey}";
        var filePath = Path.Combine(_cacheDir, key);

        var imgFile = File.Create( filePath );
        await imgFile.WriteAsync( await retVal.Tile.GetImageAsync() );

        return retVal;
    }
}
