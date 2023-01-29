using J4JSoftware.Logging;

namespace J4JMapLibrary;

[MapProjection("OpenTopoMaps", ServerConfigurationStyle.Static)]
public class OpenTopoMapsProjection : OpenMapProjection
{
    public OpenTopoMapsProjection( 
        IStaticConfiguration staticConfig, 
        IJ4JLogger logger,
        ITileCache? cache = null
    )
        : base( staticConfig, logger, cache )
    {
    }

    public OpenTopoMapsProjection(
        ILibraryConfiguration libConfiguration,
        IJ4JLogger logger,
        ITileCache? cache = null
    )
        : base(libConfiguration, logger, cache )
    {
    }
}
