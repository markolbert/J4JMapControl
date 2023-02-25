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
using System.Security.Cryptography;
using System.Text;

namespace J4JSoftware.J4JMapLibrary;

[ Projection( "GoogleMaps" ) ]
public sealed class GoogleMapsProjection : StaticProjection<GoogleCredentials>
{
    public GoogleMapsProjection(
        IJ4JLogger logger

    )
        : base( logger )
    {
        MinScale = 0;
        MaxScale = 20;
        TileHeightWidth = 256;
        Copyright = "© Google";
        CopyrightUri = new Uri("http://www.google.com");
        ImageFileExtension = ".png";

        // this doesn't have the required signature field, but that gets appended
        // when the request is created because it involves a cryptographic call
        // against the raw URL
        RetrievalUrl = "https://maps.googleapis.com/maps/api/staticmap?";
        RetrievalQueryString = "center={center}&format={format}&zoom={zoom}&size={size}&key={apikey}";
    }

    public string ApiKey { get; private set; } = string.Empty;
    public string Signature { get; private set; } = string.Empty;

    public GoogleMapType MapType { get; set; } = GoogleMapType.RoadMap;
    public GoogleImageFormat ImageFormat { get; set; } = GoogleImageFormat.Png;
    public string RetrievalUrl { get; }
    public string RetrievalQueryString { get; }

#pragma warning disable CS1998
    public override async Task<bool> AuthenticateAsync( GoogleCredentials credentials, CancellationToken ctx = default )
#pragma warning restore CS1998
    {
        Initialized = false;

        ApiKey = credentials.ApiKey;
        Signature = credentials.SignatureSecret;

        Initialized = true;

        return true;
    }

    public override HttpRequestMessage? CreateMessage(IStaticFragment mapFragment )
    {
        if (!Initialized)
        {
            Logger.Error("Trying to create image retrieval HttpRequestMessage when uninitialized");
            return null;
        }

        var replacements = new Dictionary<string, string>
        {
            { "{center}", $"{mapFragment.CenterLatitude}, {mapFragment.CenterLongitude}" },
            { "{format}", ImageFormat.ToString() },
            { "{zoom}", mapFragment.Scale.ToString() },
            { "{size}", $"{mapFragment.ImageWidth}x{mapFragment.ImageHeight}" },
            { "{apikey}", ApiKey }
        };

        var unsignedQuery = InternalExtensions.ReplaceParameters(RetrievalQueryString, replacements);
        var encodedQuery = unsignedQuery;// HttpUtility.UrlEncode(unsignedQuery);
        var unsignedUrl = $"{RetrievalUrl}{encodedQuery}";
        var signedUrl = SignUrl(unsignedUrl);

        return new HttpRequestMessage(HttpMethod.Get, new Uri(signedUrl));
    }

    public string SignUrl(string url)
    {
        var encoding = new ASCIIEncoding();

        // converting key to bytes will throw an exception, need to replace '-' and '_' characters first.
        var usablePrivateKey = Signature.Replace("-", "+")
                                        .Replace("_", "/");

        var privateKeyBytes = Convert.FromBase64String(usablePrivateKey);

        var uri = new Uri(url);
        var encodedPathAndQueryBytes = encoding.GetBytes(uri.LocalPath + uri.Query);

        // compute the hash
        var algorithm = new HMACSHA1(privateKeyBytes);
        var hash = algorithm.ComputeHash(encodedPathAndQueryBytes);

        // convert the bytes to string and make url-safe by replacing '+' and '/' characters
        var signature = Convert.ToBase64String(hash)
                               .Replace("+", "-")
                               .Replace("/", "_");

        // Add the signature to the existing URI.
        return $"{uri.Scheme}://{uri.Host}{uri.LocalPath}{uri.Query}&signature={signature}";
    }
}
