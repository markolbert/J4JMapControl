namespace J4JMapLibrary;

public interface ITiledProjection : IMapProjection
{
    int MinX { get; }
    int MaxX { get; }
    int MinY { get; }
    int MaxY { get; }
    
    int MaxScale { get; }
    int MinScale { get; }
    int Scale { get; set; }

    RectangleSize Size { get; }
    RectangleSize TileSize { get; }

    TileCoordinates MinTile { get; }
    TileCoordinates MaxTile { get; }

    double GroundResolution( double latitude );
    string MapScale( double latitude, double dotsPerInch );
}
