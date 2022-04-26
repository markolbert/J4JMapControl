using System;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;
using Microsoft.UI.Xaml.Media;

namespace J4JSoftware.J4JMapControl;

public class OpenTopoMapsImageRetriever : TileBasedImageRetriever<MultiTileCoordinates>
{
    public OpenTopoMapsImageRetriever(
        IJ4JLogger? logger
    )
        : base( "https://tile.opentopomap.org/{ZoomLevel}/{XTile}/{YTile}.png",
                "OpenTopoMap",
                "© OpenTopoMap-Mitwirkende, SRTM | Kartendarstellung\n© OpenTopoMap\nCC-BY-SA",
                new Uri( "http://opentopomap.org/" ),
                logger )
    {
    }

    protected override bool TryGetRequestUri(MultiTileCoordinates tile, out Uri? result)
    {
        result = new Uri(RetrievalUriTemplate.Replace("ZoomLevel", tile.Zoom.Level.ToString())
                                             .Replace("XTile", tile.TileCoordinates.X.ToString())
                                             .Replace("YTile", tile.TileCoordinates.Y.ToString()));

        return true;
    }
}
