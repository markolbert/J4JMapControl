using J4JSoftware.Logging;

namespace J4JMapLibrary;

[ Projection( "OpenTopoMaps" ) ]
public sealed class OpenTopoMapsProjection : OpenMapProjection
{
    public OpenTopoMapsProjection(
        IJ4JLogger logger,
        ITileCache? cache = null
    )
        : base( logger, cache )
    {
        MapServer = new OpenTopoMapServer();
        TiledScale = new TiledScale( MapServer );
    }

    public OpenTopoMapsProjection(
        IProjectionCredentials credentials,
        IJ4JLogger logger,
        ITileCache? cache = null
    )
        : base( credentials, logger, cache )
    {
        MapServer = new OpenTopoMapServer();
        TiledScale = new TiledScale( MapServer );
    }

    public override IMapServer MapServer { get; }
}
