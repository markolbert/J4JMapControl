using J4JSoftware.Logging;
using System.Net;
using System.Text;
using System.Text.Json;

namespace J4JMapLibrary;

public class BingMapProjection : TiledProjection
{
    private const string MetadataUrlTemplate =
        "http://dev.virtualearth.net/REST/V1/Imagery/Metadata/Mode?output=json&key=ApiKey";

    private const int BaseTileHeightWidth = 256;

    private readonly Random _random = new( Environment.TickCount );

    private string _apiKey = string.Empty;
    private int _scale;
    private string? _cultureCode;

    public BingMapProjection(
        IJ4JLogger logger
    )
        : base( Math.Atan( Math.Sinh( Math.PI ) ) * 180 / Math.PI,
                -Math.Atan( Math.Sinh( Math.PI ) ) * 180 / Math.PI,
                -180,
                180,
                logger )
    {
        TileWidth = 256;
        TileHeight = 256;

        SetSizes( 0 );
    }

    // this assumes *scale* has been normalized (i.e., x -> x - 1)
    private void SetSizes( int scale )
    {
        var numCells = Pow(2, scale);
        var heightWidth = BaseTileHeightWidth * numCells;

        MinX = 0;
        MaxX = heightWidth - 1;
        MinY = 0;
        MaxY = heightWidth - 1;

        MaxTile = CreateMapTileInternal(this, numCells - 1, numCells - 1 );
    }

    public BingMapType MapType { get; private set; } = BingMapType.Aerial;
    public BingImageryMetadata? Metadata { get; private set; }

    public override int Scale
    {
        get => _scale;

        set
        {
            if( !Initialized )
            {
                Logger.Error("Trying to set scale before projection is initialized, ignoring");
                return;
            }

            _scale = MapExtensions.ConformValueToRange( value, MinScale, MaxScale, "Scale", Logger );
            SetSizes( _scale - MinScale );

            foreach( var point in RegisteredPoints )
            {
                point.UpdateCartesian();
            }
        }
    }

    public async Task<bool> InitializeAsync( string apiKey, BingMapType mapType )
    {
        _apiKey = apiKey;
        MapType = mapType;
        Initialized = false;

        var uri = new Uri( MetadataUrlTemplate.Replace( "Mode", MapType.ToString() )
                                              .Replace( "ApiKey", _apiKey ) );

        var request = new HttpRequestMessage( HttpMethod.Get, uri );

        var uriText = uri.AbsoluteUri;
        var httpClient = new HttpClient();

        HttpResponseMessage? response;

        Logger.Verbose( "Attempting to retrieve Bing Maps metadata" );
        try
        {
            response = await httpClient.SendAsync( request );
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
            var error = await response.Content.ReadAsStringAsync();

            Logger.Error<string, string>(
                "Invalid response code received from {0} when retrieving Bing Maps Metadata, message was '{1}'",
                uriText,
                error );

            return false;
        }

        Logger.Verbose( "Attempting to parse Bing Maps metadata" );
        try
        {
            var respText = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            Metadata = JsonSerializer.Deserialize<BingImageryMetadata>( respText, options );
            Logger.Verbose( "Bing Maps metadata retrieved" );
        }
        catch( Exception ex )
        {
            Logger.Error<string>( "Could not parse Bing Maps metadata, message was '{0}'", ex.Message );

            return false;
        }

        if( Metadata!.PrimaryResource == null )
        {
            Logger.Error("Primary resource is not defined"  );
            return false;
        }

        TileWidth = Metadata.PrimaryResource.ImageWidth;
        TileHeight = Metadata.PrimaryResource.ImageHeight;

        MaxScale = Metadata.PrimaryResource.ZoomMax;
        MinScale = Metadata.PrimaryResource.ZoomMin;

        Initialized = true;

        Scale = MinScale;

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

    public string GetQuadKey(MapTile coordinates)
    {
        var retVal = new StringBuilder();

        for (var i = Scale - 1; i > 0; i--)
        {
            var digit = '0';
            var mask = 1 << (i - 1);

            if ((coordinates.X & mask) != 0)
                digit++;

            if ((coordinates.Y & mask) != 0)
            {
                digit++;
                digit++;
            }

            retVal.Append(digit);
        }

        return retVal.Length == 0 ? "0" : retVal.ToString();
    }

    protected override bool TryGetRequest( MapTile coordinates, out HttpRequestMessage? result )
    {
        result = null;

        if( !Initialized )
            return false;

        coordinates = Cap( coordinates )!;

        var subDomain = Metadata!.PrimaryResource!
                                 .ImageUrlSubdomains[ _random.Next( Metadata!
                                                                   .PrimaryResource!
                                                                   .ImageUrlSubdomains
                                                                   .Length ) ];

        var quadKey = GetQuadKey( coordinates );

        var uriText = Metadata!.PrimaryResource.ImageUrl.Replace( "{subdomain}", subDomain )
                               .Replace( "{quadkey}", quadKey )
                               .Replace( "{culture}", _cultureCode );

        result = new HttpRequestMessage( HttpMethod.Get, new Uri( uriText ) );

        return true;
    }
}
