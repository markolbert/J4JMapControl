using J4JSoftware.Logging;

namespace J4JMapLibrary;

public class OpenMapProjection : FixedTileProjection<FixedTileScope, string>
{
    private bool _authenticated;

    protected OpenMapProjection(
        IStaticConfiguration staticConfig,
        IMapServer mapServer,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( staticConfig, mapServer, logger, tileCache )
    {
        Scope.ScaleRange = new MinMax<int>( staticConfig.MinScale, staticConfig.MaxScale );
    }

    protected OpenMapProjection(
        ILibraryConfiguration libConfiguration,
        IMapServer mapServer,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( libConfiguration, mapServer, logger, tileCache )
    {
        if( !TryGetSourceConfiguration<IStaticConfiguration>( Name, out var srcConfig ) )
        {
            Logger.Fatal( "No configuration information for {0} was found in ILibraryConfiguration", GetType() );
            throw new ApplicationException(
                $"No configuration information for {GetType()} was found in ILibraryConfiguration" );
        }

        Scope.ScaleRange = new MinMax<int>(srcConfig!.MinScale, srcConfig.MaxScale);
    }

    public override bool Initialized => base.Initialized && _authenticated;

#pragma warning disable CS1998
    public override async Task<bool> AuthenticateAsync( string? credentials, CancellationToken cancellationToken )
#pragma warning restore CS1998
    {
        credentials ??= LibraryConfiguration?.Credentials
            .Where(x => x.Name.Equals(Name, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Key)
            .FirstOrDefault();

        if (credentials == null)
        {
            Logger.Error("No credentials provided or available");
            return false;
        }

        if (MapServer is not OpenMapServer mapServer)
        {
            Logger.Error("Undefined or inaccessible IMessageCreator, cannot initialize");
            return false;
        }

        _authenticated = false;

        if (!mapServer.Initialize(credentials))
            return false;

        SetScale(Scope.ScaleRange.Minimum);

        _authenticated = true;

        return true;
    }
}
