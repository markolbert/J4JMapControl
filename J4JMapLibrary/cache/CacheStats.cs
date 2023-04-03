namespace J4JSoftware.J4JMapLibrary;

public class CacheStats
{
    public void Initialize( CacheBase cache )
    {
        Bytes = 0;
        Entries = 0;
        Earliest = DateTime.MaxValue;
        MostRecent = DateTime.MinValue;

        foreach( var entry in cache )
        {
            Bytes += entry.ImageBytes;
            Entries++;

            if( entry.LastAccessedUtc > MostRecent )
                MostRecent = entry.LastAccessedUtc;
            {
                if( entry.LastAccessedUtc < Earliest )
                    Earliest = entry.LastAccessedUtc;
            }
        }
    }

    public void Initialize( IEnumerable<FileInfo> files )
    {
        Bytes = 0;
        Entries = 0;
        Earliest = DateTime.MaxValue;
        MostRecent = DateTime.MinValue;

        foreach( var file in files )
        {
            Bytes += file.Length;
            Entries++;

            if( file.LastAccessTimeUtc > MostRecent )
                MostRecent = file.LastAccessTimeUtc;
            {
                if( file.LastAccessTimeUtc < Earliest )
                    Earliest = file.LastAccessTimeUtc;
            }
        }
    }

    public long Bytes { get; protected set; }
    public int Entries { get; protected set; }

    // these DateTimes are in UTC
    public DateTime Earliest { get; protected set; } = DateTime.MinValue;
    public DateTime MostRecent { get; protected set; } = DateTime.MaxValue;

    public void RecordEntry( byte[]? imageData )
    {
        Entries++;

        Bytes += imageData?.Length ?? 0;

        var utcNow = DateTime.UtcNow;
        if( utcNow < Earliest )
            Earliest = utcNow;
        else
        {
            if( utcNow > MostRecent )
                MostRecent = utcNow;
        }
    }
}
