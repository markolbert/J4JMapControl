// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// 
// This file is part of ConsoleUtilities.
//
// ConsoleUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// ConsoleUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with ConsoleUtilities. If not, see <https://www.gnu.org/licenses/>.

using Serilog;
using System.Net;
using System.Text.Json;

namespace J4JSoftware.J4JMapLibrary;

[ Projection( "BingMaps" ) ]
public sealed class BingMapsProjection : TiledProjection<BingCredentials>
{
    public const string MetadataUrl =
        "http://dev.virtualearth.net/REST/V1/Imagery/Metadata/{mode}?output=json&key={apikey}";

    private readonly Random _random = new(Environment.TickCount);
    private string? _cultureCode;

    public BingMapsProjection(
        ILogger logger,
        ITileCache? tileCache = null
    )
        : base( logger )
    {
        TileCache = tileCache;
    }

    public string ApiKey { get; private set; } = string.Empty;

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
                    Logger.Error("Invalid or unsupported culture code '{0}'", value);
                else _cultureCode = value;
            }
        }
    }

    public BingMapStyle BingMapStyle { get; private set; } = BingMapStyle.RoadOnDemand;

    protected override void OnMapStyleChanged( string value )
    {
        base.OnMapStyleChanged( value );

        if( !Enum.TryParse<BingMapStyle>( value, true, out var result ) )
            return;

        BingMapStyle = result;

        // because the Bing retrieval url depends on the map style, we have to re-authenticate
        // whenever it changes
        if( Credentials != null )
            Authenticate();
    }

    public BingImageryMetadata? Metadata { get; internal set; }

    protected override async Task<bool> AuthenticateAsync( CancellationToken ctx = default )
    {
        // make sure we are decorated with a ProjectionAttribute, set up monitoring scale changes
        if( !await base.AuthenticateAsync( ctx ) )
            return false;

        Initialized = false;

        ApiKey = Credentials!.ApiKey;

        var replacements = new Dictionary<string, string>
        {
            { "{mode}", BingMapStyle.ToString() }, { "{apikey}", ApiKey }
        };

        var temp = InternalExtensions.ReplaceParameters(MetadataUrl, replacements);
        var uri = new Uri(temp);

        var request = new HttpRequestMessage(HttpMethod.Get, uri);

        var uriText = uri.AbsoluteUri;
        var httpClient = new HttpClient();

        HttpResponseMessage? response;

        Logger.Verbose("Attempting to retrieve Bing Maps metadata");

        try
        {
            response = MaxRequestLatency < 0
                ? await httpClient.SendAsync(request, ctx)
                : await httpClient.SendAsync(request, ctx)
                                  .WaitAsync(TimeSpan.FromMilliseconds(MaxRequestLatency), ctx);
        }
        catch (Exception ex)
        {
            // need to re-throw the exception to satisfy tests that look for
            // thrown exceptions
#if DEBUG
            if (MaxRequestLatency == 1)
                // ReSharper disable once PossibleIntendedRethrow
#pragma warning disable CA2200
                throw ex;
#pragma warning restore CA2200
#endif

            Logger.Error( "Could not retrieve Bing Maps Metadata from {0}, message was '{1}'",
                          uriText,
                          ex.Message );
            return false;
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var error = MaxRequestLatency < 0
                ? await response.Content.ReadAsStringAsync(ctx)
                : await response.Content.ReadAsStringAsync(ctx)
                                .WaitAsync(TimeSpan.FromMilliseconds(MaxRequestLatency), ctx);

            Logger.Error(
                "Invalid response code received from {0} when retrieving Bing Maps Metadata, message was '{1}'",
                uriText,
                error );

            return false;
        }

        Logger.Verbose("Attempting to parse Bing Maps metadata");

        try
        {
            var respText = await response.Content.ReadAsStringAsync(CancellationToken.None);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            Metadata = JsonSerializer.Deserialize<BingImageryMetadata>(respText, options);
            Logger.Verbose("Bing Maps metadata retrieved");
        }
        catch (Exception ex)
        {
            Logger.Error("Could not parse Bing Maps metadata, message was '{0}'", ex.Message);

            return false;
        }

        if (Metadata!.PrimaryResource == null)
        {
            Logger.Error("Primary resource is not defined");
            return false;
        }

        MinScale = Metadata.PrimaryResource.ZoomMin;
        MaxScale = Metadata.PrimaryResource.ZoomMax;
        TileHeightWidth = Metadata.PrimaryResource.ImageHeight;

        var urlText = Metadata.PrimaryResource.ImageUrl.Replace("{subdomain}", "subdomain")
                                   .Replace("{quadkey}", "0")
                                   .Replace("{culture}", null);

        var extUri = new Uri(urlText);
        ImageFileExtension = Path.GetExtension(extUri.LocalPath);

        Initialized = true;

        return Initialized;
    }

    public override HttpRequestMessage? CreateMessage( ITiledFragment mapFragment )
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
