using J4JSoftware.J4JMapLibrary;
using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    private ITileCache? _tileFileCache;

    public DependencyProperty FileSystemCachePathProperty = DependencyProperty.Register( nameof( FileSystemCachePath ),
        typeof( string ),
        typeof( J4JMapControl ),
        new PropertyMetadata( string.Empty ) );

    public string FileSystemCachePath
    {
        get => (string) GetValue( FileSystemCachePathProperty );

        set
        {
            SetValue( FileSystemCachePathProperty, value );
            UpdateCaching();
        }
    }

    public DependencyProperty FileSystemCacheEntriesProperty = DependencyProperty.Register(
        nameof( FileSystemCacheEntries ),
        typeof( int ),
        typeof( J4JMapControl ),
        new PropertyMetadata( DefaultFileSystemCacheEntries ) );

    public int FileSystemCacheEntries
    {
        get => (int) GetValue( FileSystemCacheEntriesProperty );

        set
        {
            SetValue( FileSystemCacheEntriesProperty, value );
            UpdateCaching();
        }
    }

    public DependencyProperty FileSystemCacheRetentionProperty = DependencyProperty.Register(
        nameof( FileSystemCacheRetention ),
        typeof( string ),
        typeof( J4JMapControl ),
        new PropertyMetadata( DefaultFileSystemCacheRetention.ToString() ) );

    public string FileSystemCacheRetention
    {
        get => (string) GetValue( FileSystemCacheRetentionProperty );

        set
        {
            SetValue( FileSystemCacheRetentionProperty, value );
            UpdateCaching();
        }
    }

    public DependencyProperty FileSystemCacheSizeProperty = DependencyProperty.Register( nameof( FileSystemCacheSize ),
        typeof( int ),
        typeof( J4JMapControl ),
        new PropertyMetadata( DefaultFileSystemCacheSize ) );

    public int FileSystemCacheSize
    {
        get => (int) GetValue( FileSystemCacheSizeProperty );

        set
        {
            SetValue( FileSystemCacheSizeProperty, value );
            UpdateCaching();
        }
    }
}
