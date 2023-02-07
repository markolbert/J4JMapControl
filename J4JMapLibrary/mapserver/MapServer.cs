using System.Reflection;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract class MapServer<TTile> : IMapServer<TTile>
where TTile : class
{
    public const int DefaultMaxRequestLatency = 500;

    private int _maxLatency = DefaultMaxRequestLatency;

    protected MapServer(
        IJ4JLogger logger
    )
    {
        Logger = logger;
        Logger.SetLoggedType(GetType());

        var attr = GetType().GetCustomAttribute<MessageCreatorAttribute>();
        SupportedProjection = attr?.SupportedProjection ?? string.Empty;

        if (!string.IsNullOrEmpty(SupportedProjection))
            return;

        Logger.Error("{0} is not decorated with a {1}, will not be accessible by projections", GetType(),
            typeof(MessageCreatorAttribute));
    }

    protected IJ4JLogger Logger { get; init; }

    public string SupportedProjection { get; init; }
    public abstract bool Initialized { get; }

    public int MinScale { get; protected set; }
    public int MaxScale { get; protected set; }
    public int TileHeightWidth { get; protected set; }

    public int MaxRequestLatency
    {
        get => _maxLatency;

        set
        {
            if (value < 0)
            {
                Logger.Error("Trying to set MaxRequestLatency < 0, defaulting to {0}", DefaultMaxRequestLatency);
                value = DefaultMaxRequestLatency;
            }

            _maxLatency = value;
        }
    }

    public string ImageFileExtension { get; protected set; } = string.Empty;
    public string Copyright { get; protected set; } = string.Empty;
    public Uri? CopyrightUri { get; protected set; }

    public abstract HttpRequestMessage? CreateMessage(TTile requestInfo);

    HttpRequestMessage? IMapServer.CreateMessage(object requestInfo)
    {
        if (requestInfo is TTile castInfo)
            return CreateMessage(castInfo);

        Logger.Error("Expected a {0} but was passed a {1}", typeof(TTile), requestInfo.GetType());
        return null;
    }
}