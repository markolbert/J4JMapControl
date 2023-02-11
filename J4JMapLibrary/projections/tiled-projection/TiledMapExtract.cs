using System.Runtime.CompilerServices;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public class TiledMapExtract : MapExtract
{
    public TiledMapExtract(
        ITiledProjection projection,
        IJ4JLogger logger
    )
        : base( projection, logger )
    {
    }

    public bool TryGetBounds( out TiledExtractBounds? bounds )
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

        bounds = new TiledExtractBounds(new TileCoordinates(minX, minY), new TileCoordinates(maxX, maxY));

        return true;
    }

    public override async IAsyncEnumerable<ITiledFragment> GetTilesAsync(int scale, [EnumeratorCancellation] CancellationToken ctx = default )
    {
        if (!TryGetBounds(out var bounds))
            yield break;

        for( var x = bounds!.UpperLeft.X; x <= bounds.LowerRight.X; x++ )
        {
            for( var y = bounds.UpperLeft.Y; y <= bounds.LowerRight.Y; y++ )
            {
                yield return await TiledFragment.CreateAsync( (ITiledProjection) Projection,
                                                           x,
                                                           y,
                                                           scale,
                                                           ctx: ctx );

            }
        }
    }
}
