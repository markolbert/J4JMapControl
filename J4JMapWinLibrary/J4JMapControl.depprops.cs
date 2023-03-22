using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    public DependencyProperty UseMemoryCacheProperty = DependencyProperty.Register( nameof( UseMemoryCache ),
        typeof( bool ),
        typeof( J4JMapControl ),
        new PropertyMetadata( true, OnCachingChanged ) );

    public DependencyProperty MemoryCacheSizeProperty = DependencyProperty.Register(nameof(MemoryCacheSize),
        typeof(int),
        typeof(J4JMapControl),
        new PropertyMetadata(DefaultMemoryCacheSize, OnCachingChanged));

    public DependencyProperty MemoryCacheEntriesProperty = DependencyProperty.Register(nameof(MemoryCacheEntries),
        typeof(int),
        typeof(J4JMapControl),
        new PropertyMetadata(DefaultMemoryCacheEntries, OnCachingChanged));

    public DependencyProperty MemoryCacheRetentionProperty = DependencyProperty.Register(nameof(MemoryCacheRetention),
        typeof(string),
        typeof(J4JMapControl),
        new PropertyMetadata(DefaultMemoryCacheRetention.ToString(), OnCachingChanged));

    public DependencyProperty FileSystemCachePathProperty = DependencyProperty.Register( nameof( FileSystemCachePath ),
        typeof( string ),
        typeof( J4JMapControl ),
        new PropertyMetadata( GetDefaultFileSystemCachePath(), OnCachingChanged ) );

    public DependencyProperty FileSystemCacheSizeProperty = DependencyProperty.Register(nameof(FileSystemCacheSize),
        typeof(int),
        typeof(J4JMapControl),
        new PropertyMetadata(DefaultFileSystemCacheSize, OnCachingChanged));

    public DependencyProperty FileSystemCacheEntriesProperty = DependencyProperty.Register(nameof(FileSystemCacheEntries),
        typeof(int),
        typeof(J4JMapControl),
        new PropertyMetadata(DefaultFileSystemCacheEntries, OnCachingChanged));

    public DependencyProperty FileSystemCacheRetentionProperty = DependencyProperty.Register(nameof(FileSystemCacheRetention),
        typeof(string),
        typeof(J4JMapControl),
        new PropertyMetadata(DefaultFileSystemCacheRetention.ToString(), OnCachingChanged));

    public DependencyProperty UpdateEventIntervalProperty = DependencyProperty.Register( nameof( UpdateEventInterval ),
        typeof( int ),
        typeof( J4JMapControl ),
        new PropertyMetadata( J4JMapControl.DefaultUpdateEventInterval ) );

    public DependencyProperty MapNameProperty = DependencyProperty.Register( nameof( MapName ),
                                                                             typeof( string ),
                                                                             typeof( J4JMapControl ),
                                                                             new PropertyMetadata( null, OnMapProjectionChanged ) );

    public DependencyProperty MapStyleProperty = DependencyProperty.Register(nameof(MapStyle),
                                                                            typeof(string),
                                                                            typeof(J4JMapControl),
                                                                            new PropertyMetadata(null, OnMapProjectionChanged));

    public DependencyProperty CenterProperty = DependencyProperty.Register( nameof( Center ),
                                                                            typeof( string ),
                                                                            typeof( J4JMapControl ),
                                                                            new PropertyMetadata(null, OnCenterChanged ) );

    public DependencyProperty MapScaleProperty = DependencyProperty.Register( nameof( MapScale ),
                                                                              typeof( double ),
                                                                              typeof( J4JMapControl ),
                                                                              new PropertyMetadata( 0.0, OnMapScaleChanged ) );

    public DependencyProperty MinScaleProperty = DependencyProperty.Register(nameof(MinScale),
                                                                             typeof(double),
                                                                             typeof(J4JMapControl),
                                                                             new PropertyMetadata(0.0, OnMinMaxScaleChanged));

    public DependencyProperty MaxScaleProperty = DependencyProperty.Register(nameof(MaxScale),
                                                                             typeof(double),
                                                                             typeof(J4JMapControl),
                                                                             new PropertyMetadata(0.0, OnMinMaxScaleChanged));

    public DependencyProperty HeadingProperty = DependencyProperty.Register( nameof( Heading ),
                                                                             typeof( string ),
                                                                             typeof( J4JMapControl ),
                                                                             new PropertyMetadata( null, OnHeadingChanged ) );

    public DependencyProperty IsValidProperty = DependencyProperty.Register( nameof( IsValid ),
                                                                             typeof( bool ),
                                                                             typeof( J4JMapControl ),
                                                                             new PropertyMetadata( false ) );
}
