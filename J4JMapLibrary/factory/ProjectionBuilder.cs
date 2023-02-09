using J4JSoftware.Logging;

// this class has to be in a different namespace from the
// assembly's main one so that we can avoid naming conflicts between
// property names and extension method names
namespace J4JMapLibrary.MapBuilder;

public class ProjectionBuilder
{
    private readonly IJ4JLogger _logger;

    public ProjectionBuilder(
        ProjectionFactory factory,
        IJ4JLogger logger
    )
    {
        _logger = logger;
        _logger.SetLoggedType( GetType() );

        Factory = factory;
    }

    public ProjectionFactory Factory { get; }
    public ITileCache? Cache { get; internal set; }
    public IMapServer? Server { get; internal set; }
    public object? Credentials { get; internal set; }
    public bool Authenticate { get; internal set; }
    public Type? ProjectionType { get; internal set; }
    public string? ProjectionName { get; internal set; }
    public int MaxRequestLatency { get; internal set; }
    public bool Buildable => !string.IsNullOrEmpty( ProjectionName ) || ProjectionType != null;

    public async Task<ProjectionCreationResult> Build( CancellationToken ctx = default )
    {
        if( !Buildable )
        {
            _logger.Error( "ProjectionBuilder not fully configured; set either ProjectionType or ProjectionName" );
            return ProjectionCreationResult.NoProjection;
        }

        Factory.Initialize();

        return string.IsNullOrEmpty( ProjectionName )
            ? await BuildFromType(ctx)
            : await BuildFromName(ctx);
    }

    private async Task<ProjectionCreationResult> BuildFromType( CancellationToken ctx )
    {
        var retVal = Credentials == null
            ? await Factory.CreateMapProjection( ProjectionType!, Cache, Server, Authenticate, ctx )
            : await Factory.CreateMapProjection( ProjectionType!, Credentials, Cache, Server, Authenticate, ctx );

        if( retVal.Projection == null )
            return retVal;

        retVal.Projection.MapServer.MaxRequestLatency = MaxRequestLatency;

        return retVal;
    }

    private async Task<ProjectionCreationResult> BuildFromName(CancellationToken ctx )
    {
        var retVal = Credentials == null
            ? await Factory.CreateMapProjection(ProjectionName!, Cache, Server, Authenticate, ctx)
            : await Factory.CreateMapProjection(ProjectionName!, Credentials, Cache, Server, Authenticate, ctx);

        if (retVal.Projection == null)
            return retVal;

        retVal.Projection.MapServer.MaxRequestLatency = MaxRequestLatency;

        return retVal;
    }
}
