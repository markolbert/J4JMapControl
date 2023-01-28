using J4JSoftware.Logging;
using System.Net;
using System.Text.Json;

namespace J4JMapLibrary;

[MapProjection("BingMaps", ServerConfiguration.Dynamic)]
public class BingMapProjection : TiledProjection
{
    // "http://dev.virtualearth.net/REST/V1/Imagery/Metadata/Mode?output=json&key=ApiKey";
    private readonly string _metadataUrlTemplate;

    private readonly Random _random = new( Environment.TickCount );

    private string? _apiKey;
    private BingMapType _mapType = BingMapType.Aerial;
    private string? _cultureCode;

    public BingMapProjection(
        IDynamicConfiguration dynamicConfig,
        IJ4JLogger logger
    )
        : base( dynamicConfig, true, logger )
    {
        _metadataUrlTemplate = dynamicConfig.MetadataRetrievalUrl;

        TileHeightWidth = 256;
        SetSizes( 0 );
    }

    public BingMapProjection(
        ILibraryConfiguration libConfiguration,
        IJ4JLogger logger
    )
        : base( libConfiguration, true, logger )
    {
        if( !TryGetSourceConfiguration<IDynamicConfiguration>( Name, out var srcConfig ) )
        {
            Logger.Fatal( "No configuration information for {0} was found in ILibraryConfiguration", GetType() );
            throw new ApplicationException(
                $"No configuration information for {GetType()} was found in ILibraryConfiguration" );
        }

        _metadataUrlTemplate = srcConfig!.MetadataRetrievalUrl;

        TileHeightWidth = 256;
        SetSizes(0);
    }

    public BingMapType MapType
    {
        get => _mapType;

        private set
        {
            _mapType = value;

            if( string.IsNullOrEmpty( _apiKey ) )
                return;

            var authenticated = false;

            Task.Run( async () => { authenticated = await Authenticate(); } );

            if( !authenticated )
                Logger.Warning("Map type was changed but subsequent authentication failed");
        }
    }

    public BingImageryMetadata? Metadata { get; private set; }

    public override async Task<bool> Authenticate( string? credentials = null )
    {
        if (string.IsNullOrEmpty(credentials) && !TryGetCredentials(Name, out credentials))
        {
            Logger.Error("No credentials were provided or found");
            return false;
        }

        _apiKey = credentials!;
        Initialized = false;

        var uri = new Uri(_metadataUrlTemplate.Replace("Mode", MapType.ToString())
                                              .Replace("ApiKey", _apiKey));

        var request = new HttpRequestMessage(HttpMethod.Get, uri);

        var uriText = uri.AbsoluteUri;
        var httpClient = new HttpClient();

        HttpResponseMessage? response;

        Logger.Verbose("Attempting to retrieve Bing Maps metadata");
        try
        {
            response = await httpClient.SendAsync(request);
        }
        catch (Exception ex)
        {
            Logger.Error<string, string>("Could not retrieve Bing Maps Metadata from {0}, message was '{1}'",
                                          uriText,
                                          ex.Message);

            return false;
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var error = await response.Content.ReadAsStringAsync();

            Logger.Error<string, string>(
                "Invalid response code received from {0} when retrieving Bing Maps Metadata, message was '{1}'",
                uriText,
                error);

            return false;
        }

        Logger.Verbose("Attempting to parse Bing Maps metadata");
        try
        {
            var respText = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            Metadata = JsonSerializer.Deserialize<BingImageryMetadata>(respText, options);
            Logger.Verbose("Bing Maps metadata retrieved");
        }
        catch (Exception ex)
        {
            Logger.Error<string>("Could not parse Bing Maps metadata, message was '{0}'", ex.Message);

            return false;
        }

        if (Metadata!.PrimaryResource == null)
        {
            Logger.Error("Primary resource is not defined");
            return false;
        }

        TileHeightWidth = Metadata.PrimaryResource.ImageWidth;

        Metrics = Metrics with
        {
            ScaleRange = new MinMax<int>( Metadata.PrimaryResource.ZoomMin, Metadata.PrimaryResource.ZoomMax )
        };

        // check to ensure we're dealing with square tiles
        if (TileHeightWidth != Metadata.PrimaryResource.ImageHeight)
        {
            Logger.Error("Tile service is not using square tiles");
            return false;
        }

        Initialized = true;

        Scale = Metrics.ScaleRange.Minimum;

        return true;
    }

    public bool SetCultureCode(string code)
    {
        if (!BingMapsCultureCodes.Default.ContainsKey(code))
        {
            Logger.Error<string>("Invalid or unsupported culture code '{0}'", code);
            return false;
        }

        _cultureCode = code;

        return true;
    }

    public override HttpRequestMessage? GetRequest( MapTile coordinates )
    {
        if( !Initialized )
            return null;

        coordinates = Cap( coordinates )!;

        var subDomain = Metadata!.PrimaryResource!
                                 .ImageUrlSubdomains[ _random.Next( Metadata!
                                                                   .PrimaryResource!
                                                                   .ImageUrlSubdomains
                                                                   .Length ) ];

        var quadKey = coordinates.GetQuadKey();

        var uriText = Metadata!.PrimaryResource.ImageUrl.Replace( "{subdomain}", subDomain )
                               .Replace( "{quadkey}", quadKey )
                               .Replace( "{culture}", _cultureCode );

        return new HttpRequestMessage( HttpMethod.Get, new Uri( uriText ) );
    }
}
