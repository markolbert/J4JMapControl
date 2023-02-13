using System.Runtime.CompilerServices;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public class TiledExtract : MapExtract
{
    public TiledExtract(
        ITiledProjection projection,
        IJ4JLogger logger
    )
        : base( projection, logger )
    {
    }

    public bool TryGetBounds( out TiledBounds? bounds )
    {
        bounds = null;

        var castTiles = Tiles.Cast<ITiledFragment>().ToList();

        if( castTiles.Count == 0 )
        {
            Logger.Error( Tiles.Any()
                              ? "MapExtract contains tiles that aren't ITiledFragment"
                              : "No tiles in the extract" );

            return false;
        }

        var minX = castTiles.Min(x => x.X);
        var maxX = castTiles.Max(x => x.X);
        var minY = castTiles.Min(x => x.Y);
        var maxY = castTiles.Max(x => x.Y);

        bounds = new TiledBounds(new TileCoordinates(minX, minY), new TileCoordinates(maxX, maxY));

        return true;
    }
}
