namespace J4JMapLibrary;

public interface ITiledProjection : IMapProjection
{
    event EventHandler<int>? ScaleChanged;

    int TileHeightWidth { get; }
    string ImageFileExtension { get; }

    ITileCache? TileCache { get; }
    int Scale { get; set; }

    float GroundResolution( float latitude );
    string MapScale( float latitude, float dotsPerInch );

    HttpRequestMessage? GetRequest( MapTile tile );
    Task<byte[]?> ExtractImageDataAsync( HttpResponseMessage response );
}
