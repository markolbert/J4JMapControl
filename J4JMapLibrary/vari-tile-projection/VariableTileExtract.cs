using J4JSoftware.Logging;

namespace J4JMapLibrary;

public class VariableTileExtract : ProjectionExtract
{
    public VariableTileExtract(
        IVariableTileProjection projection,
        IJ4JLogger logger
    )
        : base( projection, logger )
    {
    }

    public bool TryGetBounds( out ExtractBounds? bounds )
    {
        bounds = null;

        if( Tiles.Count == 0 )
        {
            Logger.Error("No tiles in the extract");
            return false;
        }

        if( Tiles.Count > 1 )
        {
            Logger.Error("More than one tile in the extract");
            return false;
        }

        var castTile = Tiles[ 0 ] as IVariableMapTile;
        if( castTile == null )
        {
            Logger.Error("Tile is not a IVariableMapTile");
            return false;
        }

        bounds = new ExtractBounds( castTile );
        return true;
    }

    public override IAsyncEnumerable<IVariableMapTile> GetTilesAsync( CancellationToken ctx = default ) =>
        throw new NotImplementedException();
}
