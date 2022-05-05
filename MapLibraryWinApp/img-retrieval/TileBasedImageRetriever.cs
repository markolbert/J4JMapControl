using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Web.Http;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;

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

        // if the rectangle is collapsed (which can happen if it's derived from a control which
        // hasn't been measured yet) return the coordinates for a single tile
        if( mapRectangle.IsCollapsed )
            yield return new PixelTileLatLong( mapRectangle.UpperLeft.Pixel.ToDoublePoint(),
                                               mapRectangle.UpperLeft.Tile.ToIntPoint(),
                                               mapRectangle.UpperLeft.LatLong,
                                               Zoom );
        else
        {
            for( var yTile = mapRectangle.UpperLeft.Tile.Y; yTile < mapRectangle.LowerRight.Tile.Y; ++yTile )
            {
                for( var xTile = mapRectangle.UpperLeft.Tile.X; xTile < mapRectangle.LowerRight.Tile.X; ++xTile )
                {
                    var tilePt = new IntPoint( xTile, yTile );
                    var pixelPt = Zoom.TileToPixel( tilePt );
                    var latLongPt = Zoom.PixelToLatLong( pixelPt );

                    yield return new PixelTileLatLong( pixelPt, tilePt, latLongPt, Zoom );
                }
            }
        }
    }

    protected override async Task<AsyncWebResult<InMemoryRandomAccessStream, HttpStatusCode>> ExtractImageDataAsync(
        HttpResponseMessage response
    )
    {
        try
        {
            using var responseStream = await response.Content.ReadAsInputStreamAsync();

            var memStream = new InMemoryRandomAccessStream();
            await RandomAccessStream.CopyAsync( responseStream, memStream );

            //var tempStream = memStream.GetInputStreamAt( 0 ).AsStreamForRead();
            //var buffer = new byte[ memStream.Size ];
            //var bytesRead = await tempStream.ReadAsync( buffer );

            //if( Convert.ToUInt64(bytesRead) != memStream.Size)
            //    return GetErrorAndLog<InMemoryRandomAccessStream>( "Failed to transfer all image data to buffer",
            //                                                       response.RequestMessage.RequestUri );

            return new AsyncWebResult<InMemoryRandomAccessStream, HttpStatusCode>( memStream, HttpStatusCode.Ok );

            //await File.WriteAllBytesAsync( "c://users/mark/desktop/tile.png", buffer );

            ////return new AsyncWebResult<InMemoryRandomAccessStream, HttpStatusCode>(
            ////    randomAccessStream,
            ////    response.StatusCode );

            //memStream.Seek( 0 );
            //var retVal = new Image();

            ////var outStream = new InMemoryRandomAccessStream();
            ////await outStream.WriteAsync( buffer.AsBuffer() );
            ////outStream.Seek( 0 );

            //var source = new BitmapImage();
            //await source.SetSourceAsync( memStream );
            //retVal.Source = source;

            //AttachedProperties.SetTileCoordinates( retVal, coordinates );

            //return new AsyncWebResult<Image, HttpStatusCode>( retVal, response.StatusCode );
        }
        catch( Exception ex )
        {
            return GetErrorAndLog<InMemoryRandomAccessStream>( $"Could not set bitmap image, message was '{ex.Message}'",
                                                               response.RequestMessage.RequestUri,
                                                               response.StatusCode );
        }
    }
}
