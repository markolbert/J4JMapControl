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
    protected List<IMapFragment> Tiles { get; } = new();

    public bool Add( IMapFragment mapFragment )
    {
        if( Tiles.Count == 0 )
        {
            Tiles.Add( mapFragment );
            return true;
        }


        if( Tiles[ 0 ].GetScope() != mapFragment.GetScope() )
            return false;

        Tiles.Add( mapFragment );
        return true;
    }

    public void Remove( IMapFragment mapFragment ) => Tiles.Remove( mapFragment );

    public void RemoveAt( int idx )
    {
        if( idx >= 0 && idx < Tiles.Count )
            Tiles.RemoveAt( idx );
    }

    public void Clear() => Tiles.Clear();

    public abstract IAsyncEnumerable<IMapFragment> GetTilesAsync( CancellationToken ctx = default );

    IAsyncEnumerable<IMapFragment> IProjectionExtract.GetTilesAsync(CancellationToken ctx) => GetTilesAsync(ctx);
}
