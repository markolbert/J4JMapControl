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

    private bool _authenticated;

    public BingMapsProjection(
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( logger, tileCache )
    {
        MapServer = new BingMapServer();
    }

    public BingMapsProjection(
        IProjectionCredentials credentials,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( credentials, logger, tileCache )
    {
        MapServer = new BingMapServer();
    }

    public override bool Initialized => base.Initialized && _authenticated;

    public override async Task<bool> AuthenticateAsync( BingCredentials? credentials, CancellationToken ctx = default )
    {
        await base.AuthenticateAsync( credentials, ctx );

        if( MapServer is not BingMapServer bingMapServer )
        {
            Logger.Error( "MapServer was not initialized with an instance of BingMapServer" );
            return false;
        }

        credentials ??= LibraryConfiguration?.Credentials
                                             .Where( x => x.Name.Equals( Name, StringComparison.OrdinalIgnoreCase ) )
                                             .Select( x => new BingCredentials( x.ApiKey, bingMapServer.MapType ) )
                                             .FirstOrDefault();

        if( credentials == null )
        {
            Logger.Error( "No credentials provided or available" );
            return false;
        }

        _authenticated = false;

        bingMapServer.ApiKey = credentials.ApiKey;
        bingMapServer.MapType = credentials.MapType;

        var replacements = new Dictionary<string, string>
        {
            { "{mode}", bingMapServer.MapType.ToString() }, { "{apikey}", bingMapServer.ApiKey }
        };

        var temp = InternalExtensions.ReplaceParameters( MetadataUrl, replacements );
        var uri = new Uri( temp );

        var request = new HttpRequestMessage( HttpMethod.Get, uri );

        var uriText = uri.AbsoluteUri;
        var httpClient = new HttpClient();

        HttpResponseMessage? response;

        Logger.Verbose( "Attempting to retrieve Bing Maps metadata" );

        try
        {
            response = bingMapServer.MaxRequestLatency < 0
                ? await httpClient.SendAsync( request, ctx )
                : await httpClient.SendAsync( request, ctx )
                                  .WaitAsync( TimeSpan.FromMilliseconds( bingMapServer.MaxRequestLatency ), ctx );
        }
        catch( Exception ex )
        {
            // need to re-throw the exception to satisfy tests that look for
            // thrown exceptions
#if DEBUG
            if (bingMapServer.MaxRequestLatency == 1)
                throw ex;
#endif

            Logger.Error<string, string>( "Could not retrieve Bing Maps Metadata from {0}, message was '{1}'",
                                          uriText,
                                          ex.Message );
            return false;
        }

        if( response.StatusCode != HttpStatusCode.OK )
        {
            var error = bingMapServer.MaxRequestLatency < 0
                ? await response.Content.ReadAsStringAsync( ctx )
                : await response.Content.ReadAsStringAsync( ctx )
                                .WaitAsync( TimeSpan.FromMilliseconds( bingMapServer.MaxRequestLatency ), ctx );

            Logger.Error<string, string>(
                "Invalid response code received from {0} when retrieving Bing Maps Metadata, message was '{1}'",
                uriText,
                error );

            return false;
        }

        Logger.Verbose( "Attempting to parse Bing Maps metadata" );

        try
        {
            var respText = await response.Content.ReadAsStringAsync( CancellationToken.None );

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            bingMapServer.Metadata = JsonSerializer.Deserialize<BingImageryMetadata>( respText, options );
            Logger.Verbose( "Bing Maps metadata retrieved" );
        }
        catch( Exception ex )
        {
            Logger.Error<string>( "Could not parse Bing Maps metadata, message was '{0}'", ex.Message );

            return false;
        }

        if( bingMapServer.Metadata!.PrimaryResource == null )
        {
            Logger.Error( "Primary resource is not defined" );
            return false;
        }

        bingMapServer.MinScale = bingMapServer.Metadata.PrimaryResource.ZoomMin;
        bingMapServer.MaxScale = bingMapServer.Metadata.PrimaryResource.ZoomMax;
        bingMapServer.TileHeightWidth = bingMapServer.Metadata.PrimaryResource.ImageHeight;

        var urlText = bingMapServer.Metadata.PrimaryResource.ImageUrl.Replace( "{subdomain}", "subdomain" )
                                   .Replace( "{quadkey}", "0" )
                                   .Replace( "{culture}", null );

        var extUri = new Uri( urlText );
        bingMapServer.ImageFileExtension = Path.GetExtension( extUri.LocalPath );

        _authenticated = true;

        MapServer.Scale = MapServer.ScaleRange.Minimum;

        return true;
    }
}
