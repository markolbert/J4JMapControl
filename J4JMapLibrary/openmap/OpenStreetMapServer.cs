using J4JSoftware.Logging;

namespace J4JMapLibrary;

[MessageCreator("OpenStreetMaps")]
public class OpenStreetMapServer : OpenMapServer
{
    public OpenStreetMapServer(
        IJ4JLogger logger
    )
        : base(logger)
    {
        MinScale = 0;
        MaxScale = 20;
        RetrievalUrl = "https://tile.openstreetmap.org/ZoomLevel/XTile/YTile.png";
        Copyright = "© OpenStreetMap Contributors";
        CopyrightUri = new Uri("http://www.openstreetmap.org/copyright");
    }
}