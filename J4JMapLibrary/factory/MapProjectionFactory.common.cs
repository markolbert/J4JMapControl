namespace J4JMapLibrary;

public partial class MapProjectionFactory
{
    private async Task<IMapProjection?> CreateWithSuppliedCredentials(
        SourceConfigurationInfo ctorInfo,
        IMapServer mapServer,
        ITileCache? tileCache,
        object credentials,
        CancellationToken cancellationToken
    )
    {
        if( !credentials.GetType().IsAssignableTo( ctorInfo.CredentialsType ) )
        {
            _logger.Error( "{0} requires {1} as a credential type but a {2} was supplied",
                           ctorInfo.MapProjectionType,
                           ctorInfo.CredentialsType,
                           credentials.GetType() );

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

    private async Task<IMapProjection?> CreateLookupCredentials(
        SourceConfigurationInfo ctorInfo,
        IMapServer mapServer,
        ITileCache? tileCache,
        CancellationToken cancellationToken
    )
    {
        IMapProjection? retVal;

        try
        {
            retVal = (IMapProjection?)Activator.CreateInstance(ctorInfo.MapProjectionType,
                                                               new object?[]
                                                               {
                                                                   _projCredentials, mapServer, _logger, tileCache
                                                               });
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
