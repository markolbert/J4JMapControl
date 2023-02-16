using System.Collections;
using J4JSoftware.Logging;
using System.Runtime.CompilerServices;

namespace J4JMapLibrary;

public abstract class MapExtract : IMapExtract
{
    protected MapExtract(
        IProjection projection,
        IJ4JLogger logger
    )
    {
        Projection = projection;

        Logger = logger;
        Logger.SetLoggedType( GetType() );
    }

    protected IJ4JLogger Logger { get; }
    protected IProjection Projection { get; }
    protected List<IMapFragment> Tiles { get; } = new();

    public bool Add( IMapFragment mapFragment )
    {
        if( Tiles.Count == 0 )
        {
            Tiles.Add( mapFragment );
            return true;
        }

        if( Tiles[ 0 ].MapServer != mapFragment.MapServer )
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

    public IEnumerator<IMapFragment> GetEnumerator() => ( (IEnumerable<IMapFragment>) Tiles ).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
