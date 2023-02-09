namespace J4JMapLibrary;

public interface IMapServer
{
    string SupportedProjection { get; }
    bool Initialized { get; }

    int MinScale { get; }
    int MaxScale { get; }
    float MaxLatitude { get; }
    float MinLatitude { get; }
    float MaxLongitude { get; }
    float MinLongitude { get; }

    int MaxRequestLatency { get; set; }
    int TileHeightWidth { get; }
    string ImageFileExtension { get; }
    
    string Copyright { get; }
    Uri? CopyrightUri { get; }

    HttpRequestMessage? CreateMessage(object requestInfo);
}

public interface IMapServer<in TTile, TAuth> : IMapServer
    where TTile : class
    where TAuth : class
{
    Task<bool> InitializeAsync( TAuth credentials, CancellationToken ctx = default );
    HttpRequestMessage? CreateMessage(TTile requestInfo);
}
