using J4JSoftware.Logging;

namespace J4JMapLibrary;

[MapProjection("OpenStreetMaps", typeof(IOpenMapServer))]
public sealed class OpenStreetMapsProjection : OpenMapProjection
{
    public OpenStreetMapsProjection( 
        IMapServer mapServer,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( mapServer, logger,tileCache )
    {
        SetSizes(0);
    }

    public OpenStreetMapsProjection(
        IProjectionCredentials credentials,
        IMapServer mapServer,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( credentials, mapServer, logger, tileCache )
    {
        SetSizes(0);
    }
}
