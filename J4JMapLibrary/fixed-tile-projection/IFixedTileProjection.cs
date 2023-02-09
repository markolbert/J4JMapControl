namespace J4JMapLibrary;

public interface IFixedTileProjection : IMapProjection
{
    int Width { get; }
    int Height { get; }

    MinMax<int> TileXRange { get; }
    MinMax<int> TileYRange { get; }

    ITileCache? TileCache { get; }

    float GroundResolution( float latitude );
    string MapScale( float latitude, float dotsPerInch );
}

public interface IFixedTileProjection<out TScope> : IFixedTileProjection
    where TScope : TileScope
{
    TScope Scope { get; }
}
