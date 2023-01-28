namespace J4JMapLibrary;

public interface ITiledProjection : IMapProjection
{
    bool CanBeCached { get; }

    int Scale { get; set; }

    int TileHeightWidth { get; }

    double GroundResolution( double latitude );
    string MapScale( double latitude, double dotsPerInch );

    HttpRequestMessage? GetRequest( MapTile tile );
    Task<MemoryStream?> ExtractImageDataAsync( HttpResponseMessage response );
}
