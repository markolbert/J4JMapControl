using J4JMapLibrary.MapBuilder;

namespace J4JMapLibrary;

public static class ProtectionBuilderExtensions
{
    public static ProjectionBuilder EnableCaching(
        this ProjectionBuilder builder,
        ITileCache cache
    )
    {
        builder.Cache = cache;
        return builder;
    }

    public static ProjectionBuilder DisableCaching(
        this ProjectionBuilder builder
    )
    {
        builder.Cache = null;
        return builder;
    }

    public static ProjectionBuilder Credentials(
        this ProjectionBuilder builder,
        object? credentials
    )
    {
        builder.Credentials = credentials;
        return builder;
    }

    public static ProjectionBuilder Authenticate(
        this ProjectionBuilder builder
    )
    {
        builder.Authenticate = true;
        return builder;
    }

    public static ProjectionBuilder SkipAuthentication(
        this ProjectionBuilder builder
    )
    {
        builder.Authenticate = false;
        return builder;
    }

    public static ProjectionBuilder Projection(
        this ProjectionBuilder builder,
        string projectionName
    )
    {
        builder.ProjectionName = projectionName;
        builder.ProjectionType = null;
        return builder;
    }

    public static ProjectionBuilder Projection(
        this ProjectionBuilder builder,
        Type projectionType
    )
    {
        builder.ProjectionName = null;
        builder.ProjectionType = projectionType;
        return builder;
    }

    public static ProjectionBuilder Projection<TProj>(
        this ProjectionBuilder builder
    )
        where TProj : IProjection =>
        builder.Projection( typeof( TProj ) );

    public static ProjectionBuilder RequestLatency(
        this ProjectionBuilder builder,
        int maxRequestLatency
    )
    {
        builder.MaxRequestLatency = maxRequestLatency;
        return builder;
    }
}
