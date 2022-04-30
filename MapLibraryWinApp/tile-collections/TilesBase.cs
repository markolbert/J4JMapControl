using System.Collections.Generic;
using System.Collections.ObjectModel;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;

namespace J4JSoftware.J4JMapControl;

public abstract class TilesBase<TCoord> : ITileCollection<TCoord>
    where TCoord : Coordinates
{
    protected TilesBase(
        IJ4JLogger? logger
    )
    {
        Logger=logger;
        Logger?.SetLoggedType( GetType() );
    }

    protected IJ4JLogger? Logger { get; }
    protected List<TCoord> TilesInternal { get; private set; } = new();

    public ReadOnlyCollection<TCoord> Tiles => TilesInternal.AsReadOnly();

    public Coordinates? this[ int row, int column ] => TryGetTile( row, column, out var retVal ) ? retVal : null;

    public int NumTiles => Tiles.Count;
    public virtual int NumRows { get; protected set; }
    public virtual int NumColumns { get; protected set; }

    public double ScreenWidth => UpperLeft == null || LowerRight == null 
        ? 0 
        : LowerRight.Screen.X - UpperLeft.Screen.X;

    public double ScreenHeight => UpperLeft == null || LowerRight == null
        ? 0
        : LowerRight.Screen.Y - UpperLeft.Screen.Y;

    public void Update( MapRect viewPort )
    {
        UpperLeft = viewPort.UpperLeft;
        LowerRight = viewPort.LowerRight;

        TilesInternal = GetRelevantTiles();
    }

    protected abstract List<TCoord> GetRelevantTiles();

    public MapPoint? UpperLeft { get; private set; }
    public MapPoint? LowerRight { get; private set; }

    public abstract bool TryGetTile( int row, int column, out TCoord? result );

    bool ITileCollection.TryGetTile( int row, int column, out Coordinates? result )
    {
        result = null;

        if( !TryGetTile( row, column, out var innerResult ) )
            return false;

        result = innerResult;

        return true;
    }
}
