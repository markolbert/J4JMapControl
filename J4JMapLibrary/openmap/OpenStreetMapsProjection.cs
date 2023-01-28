using J4JSoftware.Logging;

namespace J4JMapLibrary;

[MapProjection("OpenStreetMaps", ServerConfigurationStyle.Static)]
public class OpenStreetMapsProjection : OpenMapProjection
{
    public OpenStreetMapsProjection( 
        IStaticConfiguration staticConfig, 
        IJ4JLogger logger 
    )
        : base( staticConfig, logger )
    {
    }

    public OpenStreetMapsProjection(
        ILibraryConfiguration libConfiguration,
        IJ4JLogger logger
    )
        : base( libConfiguration, logger )
    {
    }
}
