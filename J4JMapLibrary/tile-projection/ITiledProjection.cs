namespace J4JMapLibrary;

public interface ITiledProjection : IMapProjection
{
    event EventHandler<int>? ScaleChanged;

    int Width { get; }
    int Height { get; }

    void SetScale( int scale );

    MinMax<int> TileXRange { get; }
    MinMax<int> TileYRange { get; }

    int TileHeightWidth { get; }
    string ImageFileExtension { get; }

    ITileCache? TileCache { get; }

    float GroundResolution( float latitude );
    string MapScale( float latitude, float dotsPerInch );

    HttpRequestMessage? GetRequest( MapTile tile );
    Task<byte[]?> ExtractImageDataAsync( HttpResponseMessage response );
}

public interface ITiledProjection<out TScope> : ITiledProjection
    where TScope : TiledMapScope
{
    TScope Scope { get; }
}
