using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract class ProjectionExtract : IProjectionExtract
{
    protected ProjectionExtract(
        IMapProjection projection,
        IJ4JLogger logger
        )
    {
        Projection = projection;

        Logger = logger;
        Logger.SetLoggedType(GetType());
    }

    protected IJ4JLogger Logger { get; }
    protected IMapProjection Projection { get; }
    protected List<IMapTile> Tiles { get; } = new();

    public bool Add( IMapTile tile )
    {
        if( Tiles.Count == 0 )
        {
            Tiles.Add( tile );
            return true;
        }


        if( Tiles[ 0 ].GetScope() != tile.GetScope() )
            return false;

        Tiles.Add( tile );
        return true;
    }

    public void Remove( IMapTile tile ) => Tiles.Remove( tile );

    public void RemoveAt( int idx )
    {
        if( idx >= 0 && idx < Tiles.Count )
            Tiles.RemoveAt( idx );
    }

    public void Clear() => Tiles.Clear();

    public abstract IAsyncEnumerable<IMapTile> GetTilesAsync( CancellationToken ctx = default );

    IAsyncEnumerable<IMapTile> IProjectionExtract.GetTilesAsync(CancellationToken ctx) => GetTilesAsync(ctx);
}
