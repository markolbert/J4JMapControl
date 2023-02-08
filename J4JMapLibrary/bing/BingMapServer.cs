using J4JSoftware.Logging;
using System.Net;
using System.Text.Json;

namespace J4JMapLibrary;

[MapServer("BingMaps", typeof(BingCredentials))]
public class BingMapServer : MapServer<FixedMapTile, BingCredentials>, IBingMapServer
{
    public const string MetadataUrl =
        "http://dev.virtualearth.net/REST/V1/Imagery/Metadata/Mode?output=json&key=ApiKey";

    private readonly Random _random = new(Environment.TickCount);

    private string _apiKey = string.Empty;
    private string? _cultureCode;

    public override bool Initialized => !string.IsNullOrEmpty(_apiKey) && Metadata != null;

    public BingMapType MapType { get; private set; } = BingMapType.Aerial;

    public BingImageryMetadata? Metadata { get; private set; }

    public override async Task<bool> InitializeAsync( BingCredentials credentials )
    {
        _apiKey = credentials.ApiKey;
        MapType = credentials.MapType;

        var uri = new Uri(MetadataUrl.Replace("Mode", MapType.ToString())
                                              .Replace("ApiKey", _apiKey));

        var request = new HttpRequestMessage(HttpMethod.Get, uri);

        var uriText = uri.AbsoluteUri;
        var httpClient = new HttpClient();

        HttpResponseMessage? response;

        Logger.Verbose("Attempting to retrieve Bing Maps metadata");

        try
        {
            response = MaxRequestLatency <= 0
                 ? await httpClient.SendAsync(request, CancellationToken.None)
            : await httpClient.SendAsync(request, CancellationToken.None)
                                   .WaitAsync(TimeSpan.FromMilliseconds(MaxRequestLatency), CancellationToken.None);
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
            var error = await response.Content.ReadAsStringAsync(CancellationToken.None);

            Logger.Error<string, string>(
                "Invalid response code received from {0} when retrieving Bing Maps Metadata, message was '{1}'",
                uriText,
                error);

            return false;
        }

        Logger.Verbose("Attempting to parse Bing Maps metadata");

        BingImageryMetadata? retVal;

        try
        {
            var respText = await response.Content.ReadAsStringAsync(CancellationToken.None);

            var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            retVal = JsonSerializer.Deserialize<BingImageryMetadata>(respText, options);
            Logger.Verbose("Bing Maps metadata retrieved");
        }
        catch (Exception ex)
        {
            Logger.Error<string>("Could not parse Bing Maps metadata, message was '{0}'", ex.Message);

            return false;
        }

        if (retVal!.PrimaryResource == null)
        {
            Logger.Error("Primary resource is not defined");
            return false;
        }

        var urlText = retVal
            .PrimaryResource.ImageUrl.Replace("{subdomain}", "subdomain")
            .Replace("{quadkey}", "0")
            .Replace("{culture}", null);

        ImageFileExtension = Path.GetExtension(urlText);
        TileHeightWidth = retVal.PrimaryResource!.ImageWidth;
        Metadata = retVal;

        return true;
    }

    public string? CultureCode
    {
        get => _cultureCode;

        set
        {
            if (string.IsNullOrEmpty(value))
                _cultureCode = null;
            else
            {
                if (!BingMapsCultureCodes.Default.ContainsKey(value))
                    Logger.Error<string>("Invalid or unsupported culture code '{0}'", value);
                else _cultureCode = value;
            }
        }
    }

    public override HttpRequestMessage? CreateMessage(FixedMapTile requestInfo)
    {
        if (!Initialized)
        {
            Logger.Error("Trying to create image retrieval HttpRequestMessage when uninitialized");
            return null;
        }

        var subDomain = Metadata!.PrimaryResource!
            .ImageUrlSubdomains[_random.Next(Metadata!
                .PrimaryResource!
                .ImageUrlSubdomains
                .Length)];

        var uriText = Metadata!.PrimaryResource.ImageUrl.Replace("{subdomain}", subDomain)
            .Replace("{quadkey}", requestInfo.QuadKey)
            .Replace("{culture}", _cultureCode);

        return new HttpRequestMessage(HttpMethod.Get, new Uri(uriText));
    }
}