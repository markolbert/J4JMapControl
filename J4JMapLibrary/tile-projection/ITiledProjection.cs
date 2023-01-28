namespace J4JMapLibrary;

public interface ITiledProjection : IMapProjection
{
    bool CanBeCached { get; }
    int TileHeightWidth { get; }
    string ImageFileExtension { get; }

    int Scale { get; set; }

    double GroundResolution( double latitude );
    string MapScale( double latitude, double dotsPerInch );

    HttpRequestMessage? GetRequest( MapTile tile );
    Task<MemoryStream?> ExtractImageDataAsync( HttpResponseMessage response );
}
