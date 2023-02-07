using J4JSoftware.Logging;

namespace J4JMapLibrary;

[MapProjection("OpenTopoMaps", ServerConfigurationStyle.Static)]
public sealed class OpenTopoMapsProjection : OpenMapProjection
{
    public OpenTopoMapsProjection( 
        IStaticConfiguration staticConfig,
        IMapServer mapServer,
        IJ4JLogger logger,
        ITileCache? cache = null
    )
        : base( staticConfig, mapServer, logger, cache )
    {
        SetSizes(0);
    }

    public OpenTopoMapsProjection(
        ILibraryConfiguration libConfiguration,
        IMapServer mapServer,
        IJ4JLogger logger,
        ITileCache? cache = null
    )
        : base(libConfiguration, mapServer, logger, cache )
    {
        SetSizes(0);
    }
}
