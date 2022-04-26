using System;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;
using Microsoft.UI.Xaml.Media;

namespace J4JSoftware.J4JMapControl;

public class OpenStreetMapsImageRetriever : TileBasedImageRetriever<MultiTileCoordinates>
{
    public OpenStreetMapsImageRetriever(
        IJ4JLogger? logger
    )
        : base( "https://tile.openstreetmap.org/{ZoomLevel}/{XTile}/{YTile}.png",
                "OpenStreetMaps",
                "© OpenStreetMap Contributors",
                new Uri( "http://www.openstreetmap.org/copyright" ),
                logger )
    {
    }

    protected override bool TryGetRequestUri( MultiTileCoordinates tile, out Uri? result )
    {
        result = new Uri( RetrievalUriTemplate.Replace( "ZoomLevel", tile.Zoom.Level.ToString() )
                                              .Replace( "XTile", tile.TileCoordinates.X.ToString() )
                                              .Replace( "YTile", tile.TileCoordinates.Y.ToString() ) );

        return true;
    }
}
