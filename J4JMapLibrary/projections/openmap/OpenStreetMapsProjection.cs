using J4JSoftware.Logging;

namespace J4JMapLibrary;

[ Projection( "OpenStreetMaps", typeof( IOpenMapServer ) ) ]
public sealed class OpenStreetMapsProjection : OpenMapProjection
{
    public OpenStreetMapsProjection(
        IMapServer mapServer,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( mapServer, logger, tileCache )
    {
    }

    public OpenStreetMapsProjection(
        IProjectionCredentials credentials,
        IMapServer mapServer,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( credentials, mapServer, logger, tileCache )
    {
    }
}
