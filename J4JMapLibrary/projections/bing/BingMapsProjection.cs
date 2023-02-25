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

using J4JSoftware.Logging;
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
        IJ4JLogger logger,
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
                    Logger.Error<string>("Invalid or unsupported culture code '{0}'", value);
                else _cultureCode = value;
            }
        }
    }

    public BingMapType MapType { get; private set; } = BingMapType.Aerial;
    public BingImageryMetadata? Metadata { get; internal set; }

    public override async Task<bool> AuthenticateAsync( BingCredentials credentials, CancellationToken ctx = default )
    {
        // make sure we are decorated with a ProjectionAttribute, set up monitoring scale changes
        if( !await base.AuthenticateAsync( credentials, ctx ) )
            return false;

        Initialized = false;

        ApiKey = credentials.ApiKey;
        MapType = credentials.MapType;

        var replacements = new Dictionary<string, string>
        {
            { "{mode}", MapType.ToString() }, { "{apikey}", ApiKey }
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

            Logger.Error<string, string>("Could not retrieve Bing Maps Metadata from {0}, message was '{1}'",
                                          uriText,
                                          ex.Message);
            return false;
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var error = MaxRequestLatency < 0
                ? await response.Content.ReadAsStringAsync(ctx)
                : await response.Content.ReadAsStringAsync(ctx)
                                .WaitAsync(TimeSpan.FromMilliseconds(MaxRequestLatency), ctx);

            Logger.Error<string, string>(
                "Invalid response code received from {0} when retrieving Bing Maps Metadata, message was '{1}'",
                uriText,
                error);

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
            Logger.Error<string>("Could not parse Bing Maps metadata, message was '{0}'", ex.Message);

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
