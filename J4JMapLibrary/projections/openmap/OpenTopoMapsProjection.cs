using J4JSoftware.Logging;

namespace J4JMapLibrary;

[ Projection( "OpenTopoMaps", typeof( IOpenMapServer ) ) ]
public sealed class OpenTopoMapsProjection : OpenMapProjection
{
    public OpenTopoMapsProjection(
        IMapServer mapServer,
        IJ4JLogger logger,
        ITileCache? cache = null
    )
        : base( mapServer, logger, cache )
    {
    }

    public OpenTopoMapsProjection(
        IProjectionCredentials credentials,
        IMapServer mapServer,
        IJ4JLogger logger,
        ITileCache? cache = null
    )
        : base( credentials, mapServer, logger, cache )
    {
    }
}
