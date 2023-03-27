using System.ComponentModel;
using Serilog;

namespace J4JSoftware.J4JMapLibrary.MapRegion;

public class Tile
{
    public Tile(
        MapRegion region,
        int x,
        int y
    )
    {
        Region = region;
        Logger = region.Logger.ForContext<MapTile>();

        ( X, Y ) = region.ProjectionType switch
        {
            ProjectionType.Static => ( 0, 0 ),
            ProjectionType.Tiled => ( x, y ),
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {typeof( ProjectionType )} value '{region.ProjectionType}'" )
        };
    }

    protected ILogger Logger { get; }

    public MapRegion Region { get; }

    // For Tile objects, X can be any value in the extended tile plane.
    // for MapTile objects, X can only take on values within the range of
    // minimum and maximum tile values for the scale of the MapRegion in
    // which the MapTile is created, or -1 if outside the MapRegion's 
    // horizontal limits, giving allowance for wrapping around the left
    // and right edges.

    // Wrapping is limited to including at most all the tiles
    // in the current horizontal range -- wrapping beyond that limits implies
    // the horizontal tile coordinate is outside the MapRegion.
    public int X { get; init; }
    public int Y { get; }
}
