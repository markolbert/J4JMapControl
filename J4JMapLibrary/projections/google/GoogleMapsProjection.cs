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

using System.Security.Cryptography;
using System.Text;
using J4JSoftware.J4JMapLibrary.MapRegion;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapLibrary;

[ Projection( "GoogleMaps" ) ]
public sealed class GoogleMapsProjection : StaticProjection<GoogleCredentials>
{
    public GoogleMapsProjection(
        ILoggerFactory? loggerFactory = null
    )
        : base( Enum.GetNames<GoogleMapStyle>(), loggerFactory )
    {
        MinScale = 0;
        MaxScale = 20;
        TileHeightWidth = 256;
        Copyright = "© Google";
        CopyrightUri = new Uri( "http://www.google.com" );
        ImageFileExtension = ".png";

        // this doesn't have the required signature field, but that gets appended
        // when the request is created because it involves a cryptographic call
        // against the raw URL
        RetrievalUrl = "https://maps.googleapis.com/maps/api/staticmap?";
        RetrievalQueryString = "center={center}&format={format}&zoom={zoom}&size={size}&key={apikey}";

        MapStyle = GoogleMapStyle.ToString();
    }

    public string ApiKey { get; private set; } = string.Empty;
    public string Signature { get; private set; } = string.Empty;

    public GoogleImageFormat ImageFormat { get; set; } = GoogleImageFormat.Png;
    public string RetrievalUrl { get; }
    public string RetrievalQueryString { get; }

    public GoogleMapStyle GoogleMapStyle { get; set; } = GoogleMapStyle.RoadMap;

    protected override void OnMapStyleChanged()
    {
        base.OnMapStyleChanged();

        if( MapStyle == null || !Enum.TryParse<GoogleMapStyle>( MapStyle, true, out var result ) )
        {
            // this will cause OnMapStyleChanged() to be raised again...
            MapStyle = GoogleMapStyle.RoadMap.ToString();
            return;
        }

        GoogleMapStyle = result;
    }

    protected override async Task<bool> AuthenticateAsync( CancellationToken ctx = default )
    {
        if( !await base.AuthenticateAsync( ctx ) )
            return false;

        Initialized = false;

        ApiKey = Credentials!.ApiKey;
        Signature = Credentials.SignatureSecret;

        Initialized = true;

        return true;
    }

    protected override HttpRequestMessage? CreateMessage( MapTile mapTile )
    {
        if( !Initialized )
        {
            Logger?.LogError( "Trying to create image retrieval HttpRequestMessage when uninitialized" );
            return null;
        }

        var height = (int) Math.Round( mapTile.Region.BoundingBox.Height );
        var width = (int) Math.Round( mapTile.Region.BoundingBox.Height );

        var replacements = new Dictionary<string, string>
        {
            { "{center}", $"{mapTile.Region.CenterLatitude}, {mapTile.Region.CenterLongitude}" },
            { "{format}", ImageFormat.ToString() },
            { "{zoom}", mapTile.Region.Scale.ToString() },
            { "{size}", $"{width}x{height}" },
            { "{apikey}", ApiKey }
        };

        var unsignedQuery = InternalExtensions.ReplaceParameters( RetrievalQueryString, replacements );
        var encodedQuery = unsignedQuery; // HttpUtility.UrlEncode(unsignedQuery);
        var unsignedUrl = $"{RetrievalUrl}{encodedQuery}";
        var signedUrl = SignUrl( unsignedUrl );

        return new HttpRequestMessage( HttpMethod.Get, new Uri( signedUrl ) );
    }

    public string SignUrl( string url )
    {
        var encoding = new ASCIIEncoding();

        // converting key to bytes will throw an exception, need to replace '-' and '_' characters first.
        var usablePrivateKey = Signature.Replace( "-", "+" )
                                        .Replace( "_", "/" );

        var privateKeyBytes = Convert.FromBase64String( usablePrivateKey );

        var uri = new Uri( url );
        var encodedPathAndQueryBytes = encoding.GetBytes( uri.LocalPath + uri.Query );

        // compute the hash
        var algorithm = new HMACSHA1( privateKeyBytes );
        var hash = algorithm.ComputeHash( encodedPathAndQueryBytes );

        // convert the bytes to string and make url-safe by replacing '+' and '/' characters
        var signature = Convert.ToBase64String( hash )
                               .Replace( "+", "-" )
                               .Replace( "/", "_" );

        // Add the signature to the existing URI.
        return $"{uri.Scheme}://{uri.Host}{uri.LocalPath}{uri.Query}&signature={signature}";
    }
}
