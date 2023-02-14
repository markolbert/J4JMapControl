using System.Reflection;
using System.Text;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract class MapServer<TTile, TAuth> : IMapServer<TTile, TAuth>
    where TTile : class
    where TAuth : class
{
    public const int DefaultMaxRequestLatency = 500;

    private int _minScale;
    private int _maxScale;
    private float _minLat = -MapConstants.Wgs84MaxLatitude;
    private float _maxLat = MapConstants.Wgs84MaxLatitude;
    private float _minLong = -180;
    private float _maxLong = 180;

    protected MapServer()
    {
        Logger = J4JDeusEx.GetLogger()!;
        Logger.SetLoggedType( GetType() );

        ScaleRange = new MinMax<int>( 0, 0 );
        LatitudeRange = new MinMax<float>(-90, 90);
        LongitudeRange = new MinMax<float>(-180, 180);

        var attr = GetType().GetCustomAttribute<MapServerAttribute>();
        SupportedProjection = attr?.ProjectionName ?? string.Empty;

        if( !string.IsNullOrEmpty( SupportedProjection ) )
            return;

        Logger.Error( "{0} is not decorated with a {1}, will not be accessible by projections",
                       GetType(),
                       typeof( MapServerAttribute ) );
    }

    protected IJ4JLogger Logger { get; }

    public string SupportedProjection { get; }
    public abstract bool Initialized { get; }

    public virtual int MinScale { 
        get => _minScale;

        protected set
        {
            _minScale = value;
            ScaleRange = new MinMax<int>( MinScale, MinScale );
        }
    }

    public virtual int MaxScale
    {
        get => _maxScale;

        protected set
        {
            _maxScale = value;
            ScaleRange = new MinMax<int>(MinScale, MinScale);
        }
    }

    public virtual MinMax<int> ScaleRange { get; set; }

    public float MaxLatitude
    {
        get => _maxLat;

        protected set
        {
            _maxLat = value;
            LatitudeRange = new MinMax<float>( MinLatitude, MaxLatitude );
        }
    }

    public float MinLatitude
    {
        get => _minLat;

        protected set
        {
            _minLat = value;
            LatitudeRange = new MinMax<float>(MinLatitude, MaxLatitude);
        }
    }

    public MinMax<float> LatitudeRange { get; private set; }

    public float MaxLongitude
    {
        get => _maxLong;

        protected set
        {
            _maxLong = value;
            LongitudeRange = new MinMax<float>(MinLongitude, MaxLongitude);
        }
    }

    public float MinLongitude
    {
        get => _minLong;

        protected set
        {
            _minLong = value;
            LongitudeRange = new MinMax<float>(MinLongitude, MaxLongitude);
        }
    }

    public MinMax<float> LongitudeRange { get; private set; }

    public int MaxRequestLatency { get; set; } = DefaultMaxRequestLatency;
    public virtual int TileHeightWidth { get; protected set; }
    public virtual string ImageFileExtension { get; protected set; } = string.Empty;

    public string Copyright { get; protected set; } = string.Empty;
    public Uri? CopyrightUri { get; protected set; }

    public abstract Task<bool> InitializeAsync( TAuth credentials, CancellationToken ctx = default );

    public abstract HttpRequestMessage? CreateMessage( TTile tile, int scale );

    // key value matching is case sensitive
    protected string ReplaceParameters(
        string template,
        Dictionary<string, string> values
    )
    {
        var sb = new StringBuilder(template);

        foreach( var kvp in values )
        {
            sb.Replace( kvp.Key, kvp.Value );
        }

        return sb.ToString();
    }

    HttpRequestMessage? IMapServer.CreateMessage( object requestInfo, int scale )
    {
        if( requestInfo is TTile castInfo )
            return CreateMessage( castInfo, scale );

        Logger.Error( "Expected a {0} but was passed a {1}", typeof( TTile ), requestInfo.GetType() );
        return null;
    }
}
