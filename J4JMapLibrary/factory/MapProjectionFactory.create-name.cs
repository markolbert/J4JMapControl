using System.Net;

namespace J4JMapLibrary;

public partial class MapProjectionFactory
{
    public async Task<MapCreationResult> CreateMapProjection(
        string projectionName,
        object credentials,
        ITileCache? tileCache,
        IMapServer? mapServer = null,
        bool authenticate = true,
        CancellationToken ctx = default
    )
    {
        if( !TryGetConstructorInfo( projectionName, out var ctorInfo ) )
            return MapCreationResult.NoProjection;

        if (!EnsureMapServer(ctorInfo!, ref mapServer))
            return MapCreationResult.NoProjection;

        if ( !credentials.GetType().IsAssignableTo( ctorInfo!.CredentialsType ) )
        {
            _logger.Warning("{0} requires {1} as a credential type but a {2} was supplied",
                            ctorInfo.MapProjectionType,
                            ctorInfo.CredentialsType,
                            credentials.GetType());

            return await CreateMapProjection(projectionName, tileCache, mapServer, authenticate, ctx);
        }

        var ctorArgs = new ParameterValue[]
        {
            new ParameterValue( ParameterType.MapServer, mapServer ),
            new ParameterValue( ParameterType.Logger, _logger ),
            new ParameterValue( ParameterType.TileCache, tileCache )
        };

        if (!TryCreateProjection(ctorInfo, ctorArgs, out var mapProjection))
            return MapCreationResult.NoProjection;

        if (await mapProjection!.AuthenticateAsync(credentials, ctx))
            return new MapCreationResult(mapProjection, true);

        _logger.Warning("Supplied credentials failed, attempting to use configured credentials");
        return await CreateMapProjection( projectionName, tileCache, mapServer, authenticate, ctx );
    }

    public async Task<MapCreationResult> CreateMapProjection(
        string projectionName,
        ITileCache? tileCache,
        IMapServer? mapServer = null,
        bool authenticate = true,
        CancellationToken ctx = default( CancellationToken )
    )
    {
        if( !TryGetConstructorInfo(projectionName, out var ctorInfo))
            return MapCreationResult.NoProjection;

        if (!EnsureMapServer(ctorInfo!, ref mapServer))
            return MapCreationResult.NoProjection;

        var ctorArgs = new ParameterValue[]
        {
            new ParameterValue( ParameterType.MapServer, mapServer ),
            new ParameterValue( ParameterType.Logger, _logger ),
            new ParameterValue( ParameterType.TileCache, tileCache ),
            new ParameterValue( ParameterType.Credentials, _projCredentials )
        };

        if ( !TryCreateProjectionConfigurationCredentials(ctorInfo!, ctorArgs, out var mapProjection))
            return MapCreationResult.NoProjection;

        if( !authenticate || await mapProjection!.AuthenticateAsync( null, ctx ) )
            return new MapCreationResult( mapProjection, authenticate );

        _logger.Error("Authentication of {0} instance failed", ctorInfo!.MapProjectionType);
        return new MapCreationResult( mapProjection, false );
    }

}
