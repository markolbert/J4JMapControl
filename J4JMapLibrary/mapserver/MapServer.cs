﻿using System.Reflection;
using System.Text;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract class MapServer<TTile, TAuth> : IMapServer<TTile, TAuth>
    where TTile : class
    where TAuth : class
{
    public const int DefaultMaxRequestLatency = 500;

    protected MapServer()
    {
        Logger = J4JDeusEx.GetLogger()!;
        Logger.SetLoggedType( GetType() );

        var attr = GetType().GetCustomAttribute<MapServerAttribute>();
        SupportedProjection = attr?.ProjectionName ?? string.Empty;

        if( !string.IsNullOrEmpty( SupportedProjection ) )
            return;

        Logger?.Error( "{0} is not decorated with a {1}, will not be accessible by projections",
                       GetType(),
                       typeof( MapServerAttribute ) );
    }

    protected IJ4JLogger Logger { get; }

    public string SupportedProjection { get; }
    public abstract bool Initialized { get; }

    public int MinScale { get; protected set; }
    public int MaxScale { get; protected set; }
    public float MaxLatitude { get; protected set; } = MapConstants.Wgs84MaxLatitude;
    public float MinLatitude { get; protected set; } = -MapConstants.Wgs84MaxLatitude;
    public float MaxLongitude { get; protected set; } = 180;
    public float MinLongitude { get; protected set; } = -180;
    public int MaxRequestLatency { get; set; } = DefaultMaxRequestLatency;
    public int TileHeightWidth { get; protected set; }
    public string ImageFileExtension { get; protected set; } = string.Empty;

    public string Copyright { get; protected set; } = string.Empty;
    public Uri? CopyrightUri { get; protected set; }

    public abstract Task<bool> InitializeAsync( TAuth credentials, CancellationToken ctx = default );

    public abstract HttpRequestMessage? CreateMessage( TTile tile );

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

    HttpRequestMessage? IMapServer.CreateMessage( object requestInfo )
    {
        if( requestInfo is TTile castInfo )
            return CreateMessage( castInfo );

        Logger.Error( "Expected a {0} but was passed a {1}", typeof( TTile ), requestInfo.GetType() );
        return null;
    }
}
