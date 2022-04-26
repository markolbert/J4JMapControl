using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace J4JSoftware.J4JMapControl;

public abstract class MapImageRetriever<TTile> : IRetrieveMapImage
    where TTile : TileCoordinates
{
    protected MapImageRetriever(
        string description,
        string copyrightText,
        Uri copyrightUri,
        IJ4JLogger? logger
    )
    {
        Description = description;
        CopyrightText = copyrightText;
        CopyrightUri = copyrightUri;

        Logger = logger;
        Logger?.SetLoggedType( GetType() );
    }

    protected IJ4JLogger? Logger { get; }

    public string Description { get; }
    public string CopyrightText { get; }
    public Uri CopyrightUri { get; }

    public async Task<ImageSource?> GetImageSourceAsync( TTile tile )
    {
        Logger?.Information("Beginning image retrieval from web");

        if (!TryGetClientHandler(out var clientHandler))
        {
            Logger?.Error("Failed to create HttpClientHandler");
            return null;
        }

        if( !TryGetRequestUri( tile, out var requestUri ) )
        {
            Logger?.Error( "Failed to create request Uri" );
            return null;
        }

        var uriText = requestUri!.AbsoluteUri;

        var httpClient = new HttpClient(clientHandler!);

        Logger?.Information<string>("Querying {0}", uriText);

        HttpResponseMessage? response;

        try
        {
            response = await httpClient.GetAsync(requestUri);
            Logger?.Information<string>("Got response from {0}", uriText);
        }
        catch (Exception ex)
        {
            Logger?.Error<string, string>("Image request from {0} failed, message was '{1}'",
                                           uriText,
                                           ex.Message);
            return null;
        }

        Logger?.Information<string>("Reading response from {0}", uriText);

        try
        {
            // thanx to lindexi for this
            // https://stackoverflow.com/questions/42523593/convert-byte-to-windows-ui-xaml-media-imaging-bitmapimage
            var imageBytes = await response.Content.ReadAsByteArrayAsync();
            var imageStream = imageBytes.AsBuffer().AsStream().AsRandomAccessStream();

            var decoder = await BitmapDecoder.CreateAsync(imageStream);
            imageStream.Seek(0);

            var retVal = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
            await retVal.SetSourceAsync(imageStream);

            return retVal;
        }
        catch (Exception ex)
        {
            Logger?.Error<string>("Could not set bitmap image, message was '{0}'", ex.Message);
            return null;
        }
    }


    protected abstract bool TryGetRequestUri(TTile tile, out Uri? result);

    protected virtual bool TryGetClientHandler(out HttpClientHandler? result)
    {
        result = new HttpClientHandler();

        return true;
    }

    async Task<ImageSource?> IRetrieveMapImage.GetImageSourceAsync( object tile )
    {
        if( tile is TTile castTile )
            return await GetImageSourceAsync( castTile );

        Logger?.Error( "GetMapImage requires a {0} but was provided a {1}", typeof( TTile ), tile.GetType() );

        return null;
    }
}
