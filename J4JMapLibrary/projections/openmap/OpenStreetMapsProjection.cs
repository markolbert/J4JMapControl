using J4JSoftware.Logging;

namespace J4JMapLibrary;

[ Projection( "OpenStreetMaps") ]
public sealed class OpenStreetMapsProjection : OpenMapProjection
{
    public OpenStreetMapsProjection(
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( logger, tileCache )
    {
        MapServer = new OpenStreetMapServer();
        TiledScale = new TiledScale(MapServer);
    }

    public OpenStreetMapsProjection(
        IProjectionCredentials credentials,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( credentials, logger, tileCache )
    {
        MapServer = new OpenStreetMapServer();
        TiledScale = new TiledScale(MapServer);
    }

    public override IMapServer MapServer { get; }
}
