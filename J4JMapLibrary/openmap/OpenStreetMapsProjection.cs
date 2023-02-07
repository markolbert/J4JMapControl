using J4JSoftware.Logging;

namespace J4JMapLibrary;

[MapProjection("OpenStreetMaps", ServerConfigurationStyle.Static)]
public sealed class OpenStreetMapsProjection : OpenMapProjection
{
    public OpenStreetMapsProjection( 
        IStaticConfiguration staticConfig, 
        IMapServer mapServer,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( staticConfig, mapServer, logger,tileCache )
    {
        SetSizes(0);
    }

    public OpenStreetMapsProjection(
        ILibraryConfiguration libConfiguration,
        IMapServer mapServer,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( libConfiguration, mapServer, logger, tileCache )
    {
        SetSizes(0);
    }
}
