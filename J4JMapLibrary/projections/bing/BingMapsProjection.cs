#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// BingMapsProjection.cs
//
// This file is part of JumpForJoy Software's J4JMapLibrary.
// 
// J4JMapLibrary is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JMapLibrary is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JMapLibrary. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System.Net;
using System.Text.Json;
using J4JSoftware.J4JMapLibrary.MapRegion;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapLibrary;

[ Projection( "BingMaps" ) ]
public sealed class BingMapsProjection : TiledProjection<BingCredentials>
{
    public const string MetadataUrl =
        "http://dev.virtualearth.net/REST/V1/Imagery/Metadata/{mode}?output=json&key={apikey}";

    private readonly Random _random = new( Environment.TickCount );
    private string? _cultureCode;

    public BingMapsProjection(
        ILoggerFactory? loggerFactory = null
    )
        : base( Enum.GetNames<BingMapStyle>(), loggerFactory )
    {
        MapStyle = BingMapStyle.ToString();
    }

    public string ApiKey { get; private set; } = string.Empty;

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
                    Logger?.LogError("Invalid or unsupported culture code '{0}'", value);
                else _cultureCode = value;
            }
        }
    }

    public BingMapStyle BingMapStyle { get; private set; } = BingMapStyle.RoadOnDemand;

    public BingImageryMetadata? Metadata { get; internal set; }

    protected override void OnMapStyleChanged()
    {
        base.OnMapStyleChanged();

        if( MapStyle == null || !Enum.TryParse<BingMapStyle>(MapStyle, true, out var result))
        {
            // this will cause OnMapStyleChanged() to be raised again...
            MapStyle = BingMapStyle.RoadOnDemand.ToString();
            return;
        }

        BingMapStyle = result;

        // because the Bing retrieval url depends on the map style, we have to re-authenticate
        // whenever it changes
        if( Credentials != null )
            Authenticate();
    }

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

        var temp = InternalExtensions.ReplaceParameters( MetadataUrl, replacements );
        var uri = new Uri( temp );

        var request = new HttpRequestMessage( HttpMethod.Get, uri );

        var uriText = uri.AbsoluteUri;
        var httpClient = new HttpClient();

        HttpResponseMessage? response;

        Logger?.LogTrace( "Attempting to retrieve Bing Maps metadata" );

        try
        {
            response = MaxRequestLatency < 0
                ? await httpClient.SendAsync( request, ctx )
                : await httpClient.SendAsync( request, ctx )
                                  .WaitAsync( TimeSpan.FromMilliseconds( MaxRequestLatency ), ctx );
        }
        catch( Exception ex )
        {
            // need to re-throw the exception to satisfy tests that look for
            // thrown exceptions
#if DEBUG
            if( MaxRequestLatency == 1 )

                // ReSharper disable once PossibleIntendedRethrow
#pragma warning disable CA2200
                throw ex;
#pragma warning restore CA2200
#endif

            Logger?.LogError( "Could not retrieve Bing Maps Metadata from {0}, message was '{1}'",
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

            Logger?.LogError(
                "Invalid response code received from {0} when retrieving Bing Maps Metadata, message was '{1}'",
                uriText,
                error );

            return false;
        }

        Logger?.LogTrace( "Attempting to parse Bing Maps metadata" );

        try
        {
            var respText = await response.Content.ReadAsStringAsync( CancellationToken.None );

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            Metadata = JsonSerializer.Deserialize<BingImageryMetadata>( respText, options );
            Logger?.LogTrace("Bing Maps metadata retrieved");
        }
        catch( Exception ex )
        {
            Logger?.LogError( "Could not parse Bing Maps metadata, message was '{0}'", ex.Message );
            return false;
        }

        if( Metadata!.PrimaryResource == null )
        {
            Logger?.LogError( "Primary resource is not defined" );
            return false;
        }

        MinScale = Metadata.PrimaryResource.ZoomMin;
        MaxScale = Metadata.PrimaryResource.ZoomMax;
        TileHeightWidth = Metadata.PrimaryResource.ImageHeight;

        // this used to be required...
        //var urlText = Metadata.PrimaryResource.ImageUrl.Replace("{subdomain}", "subdomain")
        //                           .Replace("{quadkey}", "0")
        //                           .Replace("{culture}", null);

        //var extUri = new Uri(urlText);
        ImageFileExtension = ".jpg"; // Path.GetExtension(extUri.LocalPath);

        Initialized = true;

        return Initialized;
    }

    protected override HttpRequestMessage? CreateMessage( MapTile mapTile )
    {
        if( !Initialized )
        {
            Logger?.LogError("Projection not initialized");
            return null;
        }

        if( !mapTile.InProjection )
        {
            Logger?.LogError("MapTile not in the projection");
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
            { "{quadkey}", mapTile.QuadKey },
            { "{culture}", _cultureCode ?? string.Empty }
        };

        var uriText = InternalExtensions.ReplaceParameters( Metadata!.PrimaryResource.ImageUrl, replacements );

        return new HttpRequestMessage( HttpMethod.Get, new Uri( uriText ) );
    }
}
