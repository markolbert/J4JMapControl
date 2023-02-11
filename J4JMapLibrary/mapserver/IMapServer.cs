namespace J4JMapLibrary;

public interface IMapServer
{
    string SupportedProjection { get; }
    bool Initialized { get; }

    int MinScale { get; }
    int MaxScale { get; }
    MinMax<int> ScaleRange { get; internal set; }

    float MaxLatitude { get; }
    float MinLatitude { get; }
    MinMax<float> LatitudeRange { get; }

    float MaxLongitude { get; }
    float MinLongitude { get; }
    MinMax<float> LongitudeRange { get; }

    int MaxRequestLatency { get; set; }
    int TileHeightWidth { get; }
    string ImageFileExtension { get; }

    string Copyright { get; }
    Uri? CopyrightUri { get; }

    HttpRequestMessage? CreateMessage( object requestInfo, int scale );
}

public interface IMapServer<in TTile, in TAuth> : IMapServer
    where TTile : class
    where TAuth : class
{
    Task<bool> InitializeAsync( TAuth credentials, CancellationToken ctx = default );
    HttpRequestMessage? CreateMessage( TTile tile, int scale );
}
