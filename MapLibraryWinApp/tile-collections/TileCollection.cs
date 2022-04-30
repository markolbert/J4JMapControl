using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Foundation;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;

namespace J4JSoftware.J4JMapControl;

public abstract class TileCollection<TCoord> : ITileCollection<TCoord>
    where TCoord : TileCoordinates
{
    protected TileCollection(
        IJ4JLogger? logger
    )
    {
        Logger=logger;
        Logger?.SetLoggedType( GetType() );
    }

    protected IJ4JLogger? Logger { get; }
    protected List<TCoord> TilesInternal { get; } = new();

    public ReadOnlyCollection<TCoord> Tiles => TilesInternal.AsReadOnly();
    public int NumTiles => TilesInternal.Count;
    public virtual int NumRows { get; protected set; }
    public virtual int NumColumns { get; protected set; }

    public void Update( MapRect viewPort )
    {
        UpperLeft = viewPort.UpperLeft;
        LowerRight = viewPort.LowerRight;

        TilesInternal.Clear();

        UpdateInternal();
    }

    protected abstract void UpdateInternal();

    public MapPoint? UpperLeft { get; private set; }
    public MapPoint? LowerRight { get; private set; }

    public abstract bool TryGetTile( int row, int column, out TCoord? result );

    bool ITileCollection.TryGetTile( int row, int column, out TileCoordinates? result )
    {
        result = null;

        if( !TryGetTile( row, column, out var innerResult ) )
            return false;

        result = innerResult;

        return true;
    }
}
