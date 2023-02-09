namespace J4JMapLibrary;

public static class ProtectionBuilderExtensions
{
    public static MapBuilder.ProjectionBuilder EnableCaching(
        this MapBuilder.ProjectionBuilder builder,
        ITileCache cache
    )
    {
        builder.Cache = cache;
        return builder;
    }

    public static MapBuilder.ProjectionBuilder DisableCaching(
        this MapBuilder.ProjectionBuilder builder
    )
    {
        builder.Cache = null;
        return builder;
    }

    public static MapBuilder.ProjectionBuilder Credentials(
        this MapBuilder.ProjectionBuilder builder,
        object? credentials
    )
    {
        builder.Credentials = credentials;
        return builder;
    }

    public static MapBuilder.ProjectionBuilder Authenticate(
        this MapBuilder.ProjectionBuilder builder
    )
    {
        builder.Authenticate = true;
        return builder;
    }

    public static MapBuilder.ProjectionBuilder SkipAuthentication(
        this MapBuilder.ProjectionBuilder builder
    )
    {
        builder.Authenticate = false;
        return builder;
    }

    public static MapBuilder.ProjectionBuilder Projection(
        this MapBuilder.ProjectionBuilder builder,
        string projectionName
    )
    {
        builder.ProjectionName = projectionName;
        builder.ProjectionType = null;
        return builder;
    }

    public static MapBuilder.ProjectionBuilder Projection(
        this MapBuilder.ProjectionBuilder builder,
        Type projectionType
    )
    {
        builder.ProjectionName = null;
        builder.ProjectionType = projectionType;
        return builder;
    }

    public static MapBuilder.ProjectionBuilder Projection<TProj>(
        this MapBuilder.ProjectionBuilder builder
    )
        where TProj : IMapProjection =>
        builder.Projection( typeof( TProj ) );

    public static MapBuilder.ProjectionBuilder RequestLatency(
        this MapBuilder.ProjectionBuilder builder,
        int maxRequestLatency
    )
    {
        builder.MaxRequestLatency = maxRequestLatency;
        return builder;
    }
}
