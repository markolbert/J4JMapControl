using System.Net;
using System.Security.Cryptography.Xml;
using System.Text.Json;

namespace J4JMapLibrary;

[ MapServer( "BingMaps", typeof( BingCredentials ) ) ]
public class BingMapServer : MapServer<TiledFragment, BingCredentials>, IBingMapServer
{
    public const string MetadataUrl =
        "http://dev.virtualearth.net/REST/V1/Imagery/Metadata/{mode}?output=json&key={apikey}";

    private readonly Random _random = new( Environment.TickCount );

    private string _apiKey = string.Empty;
    private string? _cultureCode;

    public string? CultureCode
    {
        get => _cultureCode;

        set
        {
            if( string.IsNullOrEmpty( value ) )
                _cultureCode = null;
            else
            {
                if( !BingMapsCultureCodes.Default.ContainsKey( value ) )
                    Logger.Error<string>( "Invalid or unsupported culture code '{0}'", value );
                else _cultureCode = value;
            }
        }
    }

    public override bool Initialized => !string.IsNullOrEmpty( _apiKey ) && Metadata != null;

    public BingMapType MapType { get; private set; } = BingMapType.Aerial;

    public BingImageryMetadata? Metadata { get; private set; }

    public override async Task<bool> InitializeAsync( BingCredentials credentials, CancellationToken ctx = default )
    {
        _apiKey = credentials.ApiKey;
        MapType = credentials.MapType;

        var replacements = new Dictionary<string, string>
        {
            { "{mode}", MapType.ToString() },
            { "{apikey}", _apiKey }
        };

        var temp = ReplaceParameters( MetadataUrl, replacements );
        var uri = new Uri( temp );

        var request = new HttpRequestMessage( HttpMethod.Get, uri );

        var uriText = uri.AbsoluteUri;
        var httpClient = new HttpClient();

        HttpResponseMessage? response;

        Logger.Verbose( "Attempting to retrieve Bing Maps metadata" );

        try
        {
            response = MaxRequestLatency < 0
                ? await httpClient.SendAsync( request, ctx )
                : await httpClient.SendAsync( request, ctx )
                                  .WaitAsync( TimeSpan.FromMilliseconds( MaxRequestLatency ), ctx );
        }
        catch( Exception ex )
        {
            Logger.Error<string, string>( "Could not retrieve Bing Maps Metadata from {0}, message was '{1}'",
                                          uriText,
                                          ex.Message );
            return false;
        }

        if( response.StatusCode != HttpStatusCode.OK )
        {
            var error = MaxRequestLatency < 0
                ? await response.Content.ReadAsStringAsync( ctx )
                : await response.Content.ReadAsStringAsync( ctx )
                                .WaitAsync( TimeSpan.FromMilliseconds( MaxRequestLatency ), ctx );

            Logger.Error<string, string>(
                "Invalid response code received from {0} when retrieving Bing Maps Metadata, message was '{1}'",
                uriText,
                error );

            return false;
        }

        Logger.Verbose( "Attempting to parse Bing Maps metadata" );

        BingImageryMetadata? retVal;

        try
        {
            var respText = await response.Content.ReadAsStringAsync( CancellationToken.None );

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            retVal = JsonSerializer.Deserialize<BingImageryMetadata>( respText, options );
            Logger.Verbose( "Bing Maps metadata retrieved" );
        }
        catch( Exception ex )
        {
            Logger.Error<string>( "Could not parse Bing Maps metadata, message was '{0}'", ex.Message );

            return false;
        }

        if( retVal!.PrimaryResource == null )
        {
            Logger.Error( "Primary resource is not defined" );
            return false;
        }

        var urlText = retVal
                     .PrimaryResource.ImageUrl.Replace( "{subdomain}", "subdomain" )
                     .Replace( "{quadkey}", "0" )
                     .Replace( "{culture}", null );

        var extUri = new Uri( urlText );

        ImageFileExtension = Path.GetExtension( extUri.LocalPath );
        TileHeightWidth = retVal.PrimaryResource!.ImageWidth;
        Metadata = retVal;

        return true;
    }

    public override HttpRequestMessage? CreateMessage( TiledFragment mapFragment )
    {
        if( !Initialized )
        {
            Logger.Error( "Trying to create image retrieval HttpRequestMessage when uninitialized" );
            return null;
        }

        var subDomain = Metadata!.PrimaryResource!
                                 .ImageUrlSubdomains[ _random.Next( Metadata!
                                                                   .PrimaryResource!
                                                                   .ImageUrlSubdomains
                                                                   .Length ) ];
        var replacements = new Dictionary<string, string>
        {
            { "{subdomain}", subDomain },
            { "{quadkey}", mapFragment.QuadKey },
            { "{culture}", _cultureCode ?? string.Empty },
        };

        var uriText = ReplaceParameters( Metadata!.PrimaryResource.ImageUrl, replacements );

        return new HttpRequestMessage( HttpMethod.Get, new Uri( uriText ) );
    }
}
