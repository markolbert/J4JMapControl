﻿using J4JSoftware.Logging;

namespace J4JMapLibrary;

[ MapProjection( "BingMaps", typeof( IBingMapServer ) ) ]
public sealed class BingMapsProjection : FixedTileProjection<TileScope, BingCredentials>
{
    // "http://dev.virtualearth.net/REST/V1/Imagery/Metadata/Mode?output=json&key=ApiKey";

    private bool _authenticated;

    public BingMapsProjection(
        IBingMapServer mapServer,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( mapServer, logger, tileCache )
    {
        SetSizes( 1 );
    }

    public BingMapsProjection(
        IProjectionCredentials credentials,
        IBingMapServer mapServer,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( credentials, mapServer, logger, tileCache )
    {
        SetSizes( 1 );
    }

    public override bool Initialized => base.Initialized && _authenticated;

    public override async Task<bool> AuthenticateAsync( BingCredentials? credentials, CancellationToken ctx = default )
    {
        if (MapServer is not IBingMapServer bingServer)
        {
            Logger.Error("Undefined or inaccessible IMessageCreator, cannot initialize");
            return false;
        }

        credentials ??= LibraryConfiguration?.Credentials
                                             .Where( x => x.Name.Equals( Name, StringComparison.OrdinalIgnoreCase ) )
                                             .Select( x => new BingCredentials( x.ApiKey, bingServer.MapType ) )
                                             .FirstOrDefault();

        if( credentials == null )
        {
            Logger.Error( "No credentials provided or available" );
            return false;
        }

        _authenticated = false;

        var initialized = MapServer.MaxRequestLatency < 0
            ? await bingServer.InitializeAsync( credentials, ctx )
            : await bingServer.InitializeAsync( credentials, ctx )
                              .WaitAsync( TimeSpan.FromMilliseconds( MapServer.MaxRequestLatency ), ctx );

        if( !initialized )
            return false;

        // accessing the Metadata property retrieves it
        if( bingServer.Metadata?.PrimaryResource == null )
            return false;

        Scope.ScaleRange = new MinMax<int>( bingServer.Metadata.PrimaryResource.ZoomMin,
                                            bingServer.Metadata.PrimaryResource.ZoomMax );

        // check to ensure we're dealing with square tiles
        if( MapServer.TileHeightWidth != bingServer.Metadata.PrimaryResource.ImageHeight )
        {
            Logger.Error( "Tile service is not using square tiles" );
            return false;
        }

        _authenticated = true;

        SetScale( Scope.ScaleRange.Minimum );

        return true;
    }
}
