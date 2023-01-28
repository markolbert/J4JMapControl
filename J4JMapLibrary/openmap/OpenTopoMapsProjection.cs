using J4JSoftware.Logging;

namespace J4JMapLibrary;

[MapProjection("OpenTopoMaps", ServerConfigurationStyle.Static)]
public class OpenTopoMapsProjection : OpenMapProjection
{
    public OpenTopoMapsProjection( 
        IStaticConfiguration staticConfig, 
        IJ4JLogger logger 
    )
        : base( staticConfig, logger )
    {
    }

    public OpenTopoMapsProjection(
        ILibraryConfiguration libConfiguration,
        IJ4JLogger logger
    )
        : base(libConfiguration, logger)
    {
    }
}
