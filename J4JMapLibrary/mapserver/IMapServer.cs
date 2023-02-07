namespace J4JMapLibrary;

public interface IMapServer
{
    string SupportedProjection { get; }
    bool Initialized { get; }

    int MinScale { get; }
    int MaxScale { get; }
    int MaxRequestLatency { get; set; }
    int TileHeightWidth { get; }
    string ImageFileExtension { get; }
    string Copyright { get; }
    Uri? CopyrightUri { get; }

    HttpRequestMessage? CreateMessage(object requestInfo);
}

public interface IMapServer<in TTile> : IMapServer
    where TTile : class
{
    HttpRequestMessage? CreateMessage(TTile requestInfo);
}