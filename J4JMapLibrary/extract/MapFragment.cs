﻿// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
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

using J4JSoftware.DeusEx;
using J4JSoftware.Logging;

namespace J4JSoftware.J4JMapLibrary;

public abstract class MapFragment : IMapFragment
{
    public event EventHandler? ImageChanged;

    protected MapFragment(
        IProjection projection
    )
    {
        Logger = J4JDeusEx.GetLogger();
        Logger?.SetLoggedType( GetType() );

        Projection = projection;
    }

    protected IJ4JLogger? Logger { get; }

    public IProjection Projection { get; }
    
    public float ImageHeight { get; set; }
    public float ImageWidth { get; set; }
    public bool InViewport { get; set; }

    public string FragmentId { get; init; } = string.Empty;

    public int XTile { get; init; }
    public int YTile { get; init; }
    public int Scale { get; protected set; }

    public byte[]? ImageData { get; set; }
    public long ImageBytes => ImageData?.Length <= 0 ? -1 : ImageData?.Length ?? -1;

    //public byte[]? GetImage( bool forceRetrieval = false )
    //{
    //    return Task.Run(async()=>await GetImageAsync(forceRetrieval)  ).Result;
    //}

    //public async Task<byte[]?> GetImageAsync( bool forceRetrieval = false, CancellationToken ctx = default )
    //{
    //    if( ImageData != null && !forceRetrieval )
    //        return ImageData;

    //    var wasNull = ImageData == null;

    //    ImageData = null;

    //    Logger?.Verbose( "Beginning image retrieval from web" );

    //    var request = Projection.CreateMessage( this, Scale );
    //    if( request == null )
    //    {
    //        Logger?.Error<string>( "Could not create HttpRequestMessage for mapFragment ({0})", FragmentId );
    //        if( wasNull )
    //            ImageChanged?.Invoke( this, EventArgs.Empty );

    //        return null;
    //    }

    //    var uriText = request.RequestUri!.AbsoluteUri;
    //    var httpClient = new HttpClient();

    //    Logger?.Verbose<string>( "Querying {0}", uriText );

    //    HttpResponseMessage? response;

    //    try
    //    {
    //        response = Projection.MaxRequestLatency <= 0
    //            ? await httpClient.SendAsync( request, ctx )
    //            : await httpClient.SendAsync( request, ctx )
    //                              .WaitAsync( TimeSpan.FromMilliseconds( Projection.MaxRequestLatency ), ctx );

    //        Logger?.Verbose<string>( "Got response from {0}", uriText );
    //    }
    //    catch( Exception ex )
    //    {
    //        Logger?.Error<Uri, string>( "Image request from {0} failed, message was '{1}'",
    //                                    request.RequestUri,
    //                                    ex.Message );
    //        if( wasNull )
    //            ImageChanged?.Invoke( this, EventArgs.Empty );

    //        return null;
    //    }

    //    if( response.StatusCode != HttpStatusCode.OK )
    //    {
    //        Logger?.Error<string, HttpStatusCode, string>(
    //            "Image request from {0} failed with response code {1}, message was '{2}'",
    //            uriText,
    //            response.StatusCode,
    //            await response.Content.ReadAsStringAsync( ctx ) );

    //        if( wasNull )
    //            ImageChanged?.Invoke( this, EventArgs.Empty );

    //        return null;
    //    }

    //    Logger?.Verbose<string>( "Reading response from {0}", uriText );

    //    ImageData = await ExtractImageStreamAsync( response, ctx );
    //    ImageChanged?.Invoke( this, EventArgs.Empty );

    //    if( ImageData == null )
    //        return null;

    //    return ImageData;
    //}

    //protected virtual async Task<byte[]?> ExtractImageStreamAsync(
    //    HttpResponseMessage response,
    //    CancellationToken ctx = default
    //)
    //{
    //    try
    //    {
    //        await using var responseStream = Projection.MaxRequestLatency < 0
    //            ? await response.Content.ReadAsStreamAsync( ctx )
    //            : await response.Content.ReadAsStreamAsync( ctx )
    //                            .WaitAsync( TimeSpan.FromMilliseconds( Projection.MaxRequestLatency ), ctx );

    //        var memStream = new MemoryStream();
    //        await responseStream.CopyToAsync( memStream, ctx );

    //        return memStream.ToArray();
    //    }
    //    catch( Exception ex )
    //    {
    //        Logger?.Error<Uri, string>( "Could not retrieve bitmap image stream from {0}, message was '{1}'",
    //                                    response.RequestMessage!.RequestUri!,
    //                                    ex.Message );

    //        return null;
    //    }
    //}
}
