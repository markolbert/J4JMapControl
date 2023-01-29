using J4JSoftware.Logging;

namespace J4JMapLibrary;

[MapProjection("OpenStreetMaps", ServerConfigurationStyle.Static)]
public class OpenStreetMapsProjection : OpenMapProjection
{
    public OpenStreetMapsProjection( 
        IStaticConfiguration staticConfig, 
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( staticConfig, logger,tileCache )
    {
    }

    public OpenStreetMapsProjection(
        ILibraryConfiguration libConfiguration,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( libConfiguration, logger, tileCache )
    {
    }
}
