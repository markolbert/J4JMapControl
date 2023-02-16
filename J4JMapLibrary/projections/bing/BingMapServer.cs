using System.Net;
using System.Security.Cryptography.Xml;
using System.Text.Json;

namespace J4JMapLibrary;

[ MapServer( "BingMaps" ) ]
public class BingMapServer : MapServer<TiledFragment, BingCredentials>
{
    public const string MetadataUrl =
        "http://dev.virtualearth.net/REST/V1/Imagery/Metadata/{mode}?output=json&key={apikey}";

    private readonly Random _random = new( Environment.TickCount );
    private string? _cultureCode;

    public string ApiKey { get; internal set; } = string.Empty;

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

    public override bool Initialized => !string.IsNullOrEmpty( ApiKey );

    public BingMapType MapType { get; internal set; } = BingMapType.Aerial;
    public BingImageryMetadata? Metadata { get; internal set; }

    public override HttpRequestMessage? CreateMessage( TiledFragment mapFragment, int scale )
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
            { "{culture}", _cultureCode ?? string.Empty }
        };

        var uriText = InternalExtensions.ReplaceParameters( Metadata!.PrimaryResource.ImageUrl, replacements );

        return new HttpRequestMessage( HttpMethod.Get, new Uri( uriText ) );
    }
}
