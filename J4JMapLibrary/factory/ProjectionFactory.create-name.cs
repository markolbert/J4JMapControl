namespace J4JMapLibrary;

public partial class ProjectionFactory
{
    public async Task<ProjectionCreationResult> CreateMapProjection(
        string projectionName,
        object credentials,
        ITileCache? tileCache,
        IMapServer? mapServer = null,
        bool authenticate = true,
        CancellationToken ctx = default
    )
    {
        if( !TryGetConstructorInfo( projectionName, out var ctorInfo ) )
            return ProjectionCreationResult.NoProjection;

        if( !EnsureMapServer( ctorInfo!, ref mapServer ) )
            return ProjectionCreationResult.NoProjection;

        if( !credentials.GetType().IsAssignableTo( ctorInfo!.CredentialsType ) )
        {
            _logger.Warning( "{0} requires {1} as a credential type but a {2} was supplied",
                             ctorInfo.MapProjectionType,
                             ctorInfo.CredentialsType,
                             credentials.GetType() );

            return await CreateMapProjection( projectionName, tileCache, mapServer, authenticate, ctx );
        }

        var ctorArgs = new List<ParameterValue>
        {
            new( ParameterType.MapServer, mapServer ),
            new( ParameterType.Logger, _logger ),
        };

        if (ctorInfo.IsTiled)
            ctorArgs.Add(new ParameterValue(ParameterType.TileCache, tileCache));

        if ( !TryCreateProjection( ctorInfo, ctorArgs, out var mapProjection ) )
            return ProjectionCreationResult.NoProjection;

        if( await mapProjection!.AuthenticateAsync( credentials, ctx ) )
            return new ProjectionCreationResult( mapProjection, true );

        _logger.Warning( "Supplied credentials failed, attempting to use configured credentials" );
        return await CreateMapProjection( projectionName, tileCache, mapServer, authenticate, ctx );
    }

    public async Task<ProjectionCreationResult> CreateMapProjection(
        string projectionName,
        ITileCache? tileCache,
        IMapServer? mapServer = null,
        bool authenticate = true,
        CancellationToken ctx = default
    )
    {
        if( !TryGetConstructorInfo( projectionName, out var ctorInfo ) )
            return ProjectionCreationResult.NoProjection;

        if( !EnsureMapServer( ctorInfo!, ref mapServer ) )
            return ProjectionCreationResult.NoProjection;

        var ctorArgs = new List<ParameterValue>
        {
            new( ParameterType.MapServer, mapServer ),
            new( ParameterType.Logger, _logger ),
            new( ParameterType.Credentials, ProjectionCredentials )
        };

        if( ctorInfo!.IsTiled )
            ctorArgs.Add( new ParameterValue( ParameterType.TileCache, tileCache ) );

        if( !TryCreateProjectionConfigurationCredentials( ctorInfo!, ctorArgs, out var mapProjection ) )
            return ProjectionCreationResult.NoProjection;

        if( !authenticate || await mapProjection!.AuthenticateAsync( null, ctx ) )
            return new ProjectionCreationResult( mapProjection, authenticate );

        _logger.Error( "Authentication of {0} instance failed", ctorInfo!.MapProjectionType );
        return new ProjectionCreationResult( mapProjection, false );
    }
}
