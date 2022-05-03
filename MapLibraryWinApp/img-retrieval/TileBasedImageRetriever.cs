using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Storage.Streams;
using Windows.Web.Http;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace J4JSoftware.J4JMapControl;

public abstract class TileBasedImageRetriever : MapImageRetriever<PixelTileLatLong>
{
    protected TileBasedImageRetriever(
        IJ4JLogger? logger
    )
        : base( logger )
    {
    }

    protected override IEnumerable<PixelTileLatLong> GetCoordinateIterator( MapRect mapRectangle )
    {
        if( Zoom == null )
        {
            Logger?.Error("Trying to iterate over a {0} with an undefined {1}", typeof(MapRect), typeof(IZoom));
            yield break;
        }

        for( var yTile = mapRectangle.UpperLeft.Tile.Y; yTile < mapRectangle.LowerRight.Tile.Y; ++yTile )
        {
            for( var xTile = mapRectangle.UpperLeft.Tile.X; xTile < mapRectangle.LowerRight.Tile.X; ++xTile )
            {
                var tilePt = new IntPoint( xTile, yTile );
                var screenPt = Zoom.TileToScreen( tilePt );
                var latLongPt = Zoom.ScreenToLatLong( screenPt );

                yield return new PixelTileLatLong( screenPt, tilePt, latLongPt, Zoom );
            }
        }
    }

    protected override async Task<AsyncWebResult<Image, HttpStatusCode>> ExtractMapImageAsync(
        HttpResponseMessage response,
        PixelTileLatLong coordinates
    )
    {
        try
        {
            using var responseStream = await response.Content.ReadAsInputStreamAsync();
            var randomAccessStream = new InMemoryRandomAccessStream();

            await RandomAccessStream.CopyAsync( responseStream, randomAccessStream );
            randomAccessStream.Seek( 0 );

            //return new AsyncWebResult<InMemoryRandomAccessStream, HttpStatusCode>(
            //    randomAccessStream,
            //    response.StatusCode );

            var retVal = new Image();

            var source = new BitmapImage();
            await source.SetSourceAsync( randomAccessStream );

            retVal.Source = source;
            AttachedProperties.SetTileCoordinates( retVal, coordinates );

            return new AsyncWebResult<Image, HttpStatusCode>( retVal, response.StatusCode );
        }
        catch( Exception ex )
        {
            Logger?.Error<string>( "Could not set bitmap image, message was '{0}'", ex.Message );

            return new AsyncWebResult<Image, HttpStatusCode>( null,
                                                              response.StatusCode,
                                                              response.RequestMessage.RequestUri.AbsoluteUri,
                                                              $"Could not set bitmap image, message was '{ex.Message}'" );
        }
    }
}
