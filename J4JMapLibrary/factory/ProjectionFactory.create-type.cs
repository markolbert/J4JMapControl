namespace J4JMapLibrary;

public partial class ProjectionFactory
{
    public async Task<ProjectionCreationResult> CreateMapProjection(
        Type projectionType,
        object credentials,
        ITileCache? tileCache,
        IMapServer? mapServer = null,
        bool authenticate = true,
        CancellationToken ctx = default
    )
    {
        if( !TryGetConstructorInfo( projectionType, out var ctorInfo ) )
            return ProjectionCreationResult.NoProjection;

        var ctorArgs = new List<ParameterValue>
        {
            //new( ParameterType.MapServer, mapServer ),
            new( ParameterType.Logger, _logger ),
        };

        if( ctorInfo!.IsTiled )
            ctorArgs.Add( new ParameterValue( ParameterType.TileCache, tileCache ) );

        if( !TryCreateProjection( ctorInfo, ctorArgs, out var mapProjection ) )
            return ProjectionCreationResult.NoProjection;

        if( await mapProjection!.AuthenticateAsync( credentials, ctx ) )
            return new ProjectionCreationResult( mapProjection, true );

        _logger.Warning( "Supplied credentials failed, attempting to use configured credentials" );
        return await CreateMapProjection( projectionType, tileCache, mapServer, authenticate, ctx );
    }

    public async Task<ProjectionCreationResult> CreateMapProjection(
        Type projectionType,
        ITileCache? tileCache,
        IMapServer? mapServer = null,
        bool authenticate = true,
        CancellationToken ctx = default
    )
    {
        if( !TryGetConstructorInfo( projectionType, out var ctorInfo ) )
            return ProjectionCreationResult.NoProjection;

        //if( !EnsureMapServer( ctorInfo!, ref mapServer ) )
        //    return ProjectionCreationResult.NoProjection;

        var ctorArgs = new List<ParameterValue>
        {
            //new( ParameterType.MapServer, mapServer ),
            new( ParameterType.Logger, _logger ), new( ParameterType.Credentials, ProjectionCredentials )
        };

        if( ctorInfo!.IsTiled )
            ctorArgs.Add( new ParameterValue( ParameterType.TileCache, tileCache ) );

        if( !TryCreateProjectionConfigurationCredentials( ctorInfo, ctorArgs, out var mapProjection ) )
            return ProjectionCreationResult.NoProjection;

        if( !authenticate || await mapProjection!.AuthenticateAsync( null, ctx ) )
            return new ProjectionCreationResult( mapProjection, authenticate );

        _logger.Error( "Authentication of {0} instance failed", ctorInfo.MapProjectionType );
        return ProjectionCreationResult.NoProjection;
    }
}
