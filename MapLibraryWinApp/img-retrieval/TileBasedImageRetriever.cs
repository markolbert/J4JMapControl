using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Web.Http;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;

namespace J4JSoftware.J4JMapControl;

public abstract class TileBasedImageRetriever : MapImageRetriever
{
    protected TileBasedImageRetriever(
        IJ4JLogger? logger
    )
        : base( true, logger )
    {
    }

    protected override async Task<AsyncWebResult<InMemoryRandomAccessStream>> ExtractImageDataAsync(
        HttpResponseMessage response
    )
    {
        try
        {
            using var responseStream = await response.Content.ReadAsInputStreamAsync();

            var memStream = new InMemoryRandomAccessStream();
            await RandomAccessStream.CopyAsync( responseStream, memStream );

            return new AsyncWebResult<InMemoryRandomAccessStream>( memStream, (int) HttpStatusCode.Ok );
        }
        catch( Exception ex )
        {
            return GetErrorAndLog<InMemoryRandomAccessStream>( $"Could not set bitmap image, message was '{ex.Message}'",
                                                               response.RequestMessage.RequestUri,
                                                               response.StatusCode );
        }
    }
}
