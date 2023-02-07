using J4JSoftware.Logging;

namespace J4JMapLibrary;

[MapProjection("BingMaps", ServerConfigurationStyle.Dynamic)]
public sealed class BingMapsProjection : FixedTileProjection<FixedTileScope, BingCredentials>
{
    // "http://dev.virtualearth.net/REST/V1/Imagery/Metadata/Mode?output=json&key=ApiKey";

    private bool _authenticated;

    public BingMapsProjection(
        IDynamicConfiguration dynamicConfig,
        IBingMapServer mapServer,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( dynamicConfig, mapServer , logger, tileCache )
    {
        SetSizes( 1 );
    }

    public BingMapsProjection(
        ILibraryConfiguration libConfiguration,
        IBingMapServer mapServer,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( libConfiguration,mapServer, logger, tileCache )
    {
        if( !TryGetSourceConfiguration<IDynamicConfiguration>( Name, out _ ) )
        {
            Logger.Fatal( "No configuration information for {0} was found in ILibraryConfiguration", GetType() );
            throw new ApplicationException(
                $"No configuration information for {GetType()} was found in ILibraryConfiguration" );
        }

        SetSizes( 1 );
    }

    public override bool Initialized => base.Initialized && _authenticated;

    public override async Task<bool> AuthenticateAsync(  BingCredentials? credentials, CancellationToken cancellationToken)
    {
        credentials ??= LibraryConfiguration?.Credentials
            .Where(x => x.Name.Equals(Name, StringComparison.OrdinalIgnoreCase))
            .Select(x => new BingCredentials(x.Key, BingMapType.Aerial))
            .FirstOrDefault();

        if (credentials == null)
        {
            Logger.Error("No credentials provided or available");
            return false;
        }

        if (MapServer is not BingMapServer bingServer)
        {
            Logger.Error("Undefined or inaccessible IMessageCreator, cannot initialize");
            return false;
        }

        _authenticated = false;

        if (!await bingServer.InitializeAsync(credentials))
            return false;

        // accessing the Metadata property retrieves it
        if (bingServer.Metadata?.PrimaryResource == null )
            return false;

        Scope.ScaleRange = new MinMax<int>(bingServer.Metadata.PrimaryResource.ZoomMin,
            bingServer.Metadata.PrimaryResource.ZoomMax);

        // check to ensure we're dealing with square tiles
        if (MapServer.TileHeightWidth != bingServer.Metadata.PrimaryResource.ImageHeight)
        {
            Logger.Error("Tile service is not using square tiles");
            return false;
        }

        _authenticated= true;

        SetScale( Scope.ScaleRange.Minimum );

        return true;
    }
}
