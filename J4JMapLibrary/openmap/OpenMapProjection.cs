using J4JSoftware.Logging;

namespace J4JMapLibrary;

public class OpenMapProjection : TiledProjection
{
    private readonly string _retrievalUrl;

    private string _userAgent = string.Empty;

    protected OpenMapProjection(
        IStaticConfiguration staticConfig,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( staticConfig, logger, tileCache )
    {
        _retrievalUrl = staticConfig.RetrievalUrl;
        SetImageFileExtension( _retrievalUrl );

        TileHeightWidth = staticConfig.TileHeightWidth;
        Metrics = Metrics with { ScaleRange = new MinMax<int>( staticConfig.MinScale, staticConfig.MaxScale ) };

        SetSizes( 0 );
    }

    protected OpenMapProjection(
        ILibraryConfiguration libConfiguration,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( libConfiguration, logger, tileCache )
    {
        if( !TryGetSourceConfiguration<IStaticConfiguration>( Name, out var srcConfig ) )
        {
            Logger.Fatal( "No configuration information for {0} was found in ILibraryConfiguration", GetType() );
            throw new ApplicationException(
                $"No configuration information for {GetType()} was found in ILibraryConfiguration" );
        }

        _retrievalUrl = srcConfig!.RetrievalUrl;
        SetImageFileExtension( _retrievalUrl );

        TileHeightWidth = srcConfig.TileHeightWidth;
        Metrics = Metrics with { ScaleRange = new MinMax<int>(srcConfig.MinScale, srcConfig.MaxScale) };

        SetSizes( 0 );
    }

#pragma warning disable CS1998
    public override async Task<bool> Authenticate( CancellationToken cancellationToken, string? credentials = null )
#pragma warning restore CS1998
    {
        if( string.IsNullOrEmpty( credentials ) && !TryGetCredentials( Name, out credentials ) )
        {
            Logger.Error("No credentials were provided or found");
            return false;
        }

        _userAgent = credentials!;
        Initialized = true;

        Scale = Metrics.ScaleRange.Minimum;

        return true;
    }

    public override async Task<HttpRequestMessage?> GetRequestAsync( MapTile coordinates )
    {
        if( !Initialized )
            return null;

        coordinates = (await CapAsync( coordinates ))!;

        if( string.IsNullOrEmpty( _userAgent ) )
        {
            Logger.Error( "Undefined or empty User-Agent" );
            return null;
        }

        var uriText = _retrievalUrl.Replace( "ZoomLevel", Scale.ToString() )
                                   .Replace( "XTile", coordinates.X.ToString() )
                                   .Replace( "YTile", coordinates.Y.ToString() );

        var retVal = new HttpRequestMessage( HttpMethod.Get, new Uri( uriText ) );
        retVal.Headers.Add( "User-Agent", _userAgent );

        return retVal;
    }
}
