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
    public int X { get; }
    public int Y { get; }
}
