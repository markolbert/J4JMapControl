namespace J4JMapLibrary;

public interface ITiledProjection : IMapProjection
{
    int TileHeightWidth { get; }
    string ImageFileExtension { get; }

    ITileCache? TileCache { get; }
    int Scale { get; set; }

    double GroundResolution( double latitude );
    string MapScale( double latitude, double dotsPerInch );

    HttpRequestMessage? GetRequest( MapTile tile );
    Task<byte[]?> ExtractImageDataAsync( HttpResponseMessage response );
}
