using J4JSoftware.Logging;

namespace J4JMapLibrary;

public class OpenMapProjection : FixedTileProjection<FixedTileScope, string>
{
    private bool _authenticated;

    protected OpenMapProjection(
        IMapServer mapServer,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( mapServer, logger, tileCache )
    {
        Scope.ScaleRange = new MinMax<int>(mapServer.MinScale, mapServer.MaxScale);
    }

    protected OpenMapProjection(
        IProjectionCredentials credentials,
        IMapServer mapServer,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( credentials, mapServer, logger, tileCache )
    {
        Scope.ScaleRange = new MinMax<int>(mapServer.MinScale, mapServer.MaxScale);
    }

    public override bool Initialized => base.Initialized && _authenticated;

#pragma warning disable CS1998
    public override async Task<bool> AuthenticateAsync( string? credentials, CancellationToken ctx )
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

        if (MapServer is not IOpenMapServer mapServer)
        {
            Logger.Error("Undefined or inaccessible IMessageCreator, cannot initialize");
            return false;
        }

        _authenticated = false;

        if (!await mapServer.InitializeAsync(credentials, ctx))
            return false;

        SetScale(Scope.ScaleRange.Minimum);

        _authenticated = true;

        return true;
    }
}
