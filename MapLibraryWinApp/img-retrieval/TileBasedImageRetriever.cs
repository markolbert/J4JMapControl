using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace J4JSoftware.J4JMapControl;

public abstract class TileBasedImageRetriever<TMultiTile> : MapImageRetriever<TMultiTile>
    where TMultiTile : MultiTileCoordinates
{
    protected TileBasedImageRetriever(
        string retrievalUriTemplate,
        string description,
        string copyrightText,
        Uri copyrightUri,
        IJ4JLogger? logger
    )
        : base( description, copyrightText, copyrightUri, logger )
    {
        RetrievalUriTemplate = retrievalUriTemplate;
    }

    public string RetrievalUriTemplate { get; }
}
