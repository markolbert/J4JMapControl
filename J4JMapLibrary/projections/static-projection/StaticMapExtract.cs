using J4JSoftware.Logging;

namespace J4JMapLibrary;

public class StaticMapExtract : MapExtract
{
    public StaticMapExtract(
        IStaticProjection projection,
        IJ4JLogger logger
    )
        : base( projection, logger )
    {
    }

    public bool TryGetBounds( out StaticBounds? bounds )
    {
        bounds = null;

        if( Tiles.Count == 0 )
        {
            Logger.Error("No tiles in the extract");
            return false;
        }

        if( Tiles.Count > 1 )
        {
            Logger.Error("More than one mapFragment in the extract");
            return false;
        }

        var castTile = Tiles[ 0 ] as IStaticFragment;
        if( castTile == null )
        {
            Logger.Error("Tile is not a IStaticFragment");
            return false;
        }

        bounds = new StaticBounds( castTile );
        return true;
    }

    public override IAsyncEnumerable<IStaticFragment> GetTilesAsync(int scale, CancellationToken ctx = default ) =>
        throw new NotImplementedException();
}
