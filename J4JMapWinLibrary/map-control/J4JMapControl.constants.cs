using System;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    private const int DefaultMemoryCacheSize = 1000000;
    private const int DefaultMemoryCacheEntries = 500;
    private const int DefaultFileSystemCacheSize = 10000000;
    private const int DefaultFileSystemCacheEntries = 1000;
    internal const int DefaultUpdateEventInterval = 250;
    private const int DefaultControlHeight = 300;
    private static readonly TimeSpan DefaultMemoryCacheRetention = new( 1, 0, 0 );
    private static readonly TimeSpan DefaultFileSystemCacheRetention = new( 1, 0, 0, 0 );
}
