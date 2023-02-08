using System.Net;

namespace J4JMapLibrary;

public partial class MapProjectionFactory
{
    public async Task<IMapProjection?> CreateMapProjection(
        string projectionName,
        object credentials,
        IMapServer? mapServer,
        ITileCache? tileCache,
        CancellationToken cancellationToken
    )
    {
        var ctorInfo = _sources.Values
                               .FirstOrDefault( x => x.Name.Equals( projectionName ) );

        if( ctorInfo == null )
        {
            _logger.Error<string>( "{0} is not a known map projection name", projectionName );
            return null;
        }

        if( !credentials.GetType().IsAssignableTo( ctorInfo.CredentialsType ) )
        {
            _logger.Error( "Supplied credentials are a {0} but something assignable to a {1} is required",
                           credentials.GetType(),
                           ctorInfo.CredentialsType );
            return null;
        }

        mapServer ??= Activator.CreateInstance( ctorInfo.ServerType ) as IMapServer;
        if( mapServer == null )
        {
            _logger.Error( "Could not create an instance of {0}", ctorInfo.ServerType );
            return null;
        }

        IMapProjection? retVal;

        try
        {
            retVal = (IMapProjection?) Activator.CreateInstance( ctorInfo.MapProjectionType,
                                                                 new object?[] { mapServer, _logger, tileCache } );
        }
        catch( Exception ex )
        {
            _logger.Error<Type, string>( "Failed to create instance of {0}, message was '{1}'",
                                         ctorInfo.MapProjectionType,
                                         ex.Message );
            return null;
        }

        if( retVal == null )
        {
            _logger.Error( "Failed to create instance of {0}", ctorInfo.MapProjectionType );
            return null;
        }

        if( await retVal.AuthenticateAsync( credentials, cancellationToken ) )
            return retVal;

        _logger.Error( "Authentication of {0} instance failed", ctorInfo.MapProjectionType );
        return null;
    }

    public async Task<IMapProjection?> CreateMapProjection(
        string projectionName,
        IMapServer? mapServer = null,
        ITileCache? tileCache = null,
        CancellationToken cancellationToken = default( CancellationToken )
    )
    {
        var ctorInfo = _sources.Values
                               .FirstOrDefault(x => x.Name.Equals(projectionName));

        if (ctorInfo == null)
        {
            _logger.Error<string>("{0} is not a known map projection name", projectionName);
            return null;
        }

        if (!ctorInfo.HasCredentialsConstructor)
        {
            _logger.Error("{0} does not have a constructor accepting a {1}",
                           ctorInfo.MapProjectionType,
                           typeof(ICredentials));
            return null;
        }

        mapServer ??= Activator.CreateInstance(ctorInfo.ServerType) as IMapServer;
        if (mapServer == null)
        {
            _logger.Error("Could not create an instance of {0}", ctorInfo.ServerType);
            return null;
        }

        IMapProjection? retVal;

        try
        {
            retVal = (IMapProjection?)Activator.CreateInstance(ctorInfo.MapProjectionType,
                                                      new object?[] { mapServer, _logger, tileCache });
        }
        catch (Exception ex)
        {
            _logger.Error<Type, string>("Failed to create instance of {0}, message was '{1}'",
                                        ctorInfo.MapProjectionType,
                                        ex.Message);
            return null;
        }

        if (retVal == null)
        {
            _logger.Error("Failed to create instance of {0}", ctorInfo.MapProjectionType);
            return null;
        }

        if (await retVal.AuthenticateAsync(null, cancellationToken))
            return retVal;

        _logger.Error("Authentication of {0} instance failed", ctorInfo.MapProjectionType);
        return null;
    }

}
