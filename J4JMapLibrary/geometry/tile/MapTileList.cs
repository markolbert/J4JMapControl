using System.Collections;

namespace J4JMapLibrary;

public class MapTileList : IEnumerable<MapTile>
{
    public record Bounds( MapTile UpperLeft, MapTile LowerRight );

    private readonly List<MapTile> _tiles = new();

    public bool Add( MapTile mapTile )
    {
        if( _tiles.Count == 0 )
        {
            _tiles.Add( mapTile );
            return true;
        }

        if( !MapTile.InSameProjectionScope( _tiles[ 0 ], mapTile ) )
            return false;

        _tiles.Add( mapTile );
        return true;
    }

    public void Remove( MapTile mapTile ) => _tiles.Remove( mapTile );

    public void RemoveAt( int idx )
    {
        if( idx >= 0 && idx < _tiles.Count )
            _tiles.RemoveAt( idx );
    }

    public void Clear() => _tiles.Clear();

    public async Task<List<MapTile>> GetBoundingBoxAsync(
        ITiledProjection projection,
        CancellationToken cancellationToken
    )
    {
        var retVal = new List<MapTile>();

        if( _tiles.Count == 0 )
            return retVal;

        var minX = _tiles.Min( x => x.X );
        var maxX = _tiles.Max( x => x.X );
        var minY = _tiles.Min( x => x.Y );
        var maxY = _tiles.Max( x => x.Y );

        for( var x = minX; x <= maxX; x++ )
        {
            for( var y = minY; y <= maxY; y++ )
            {
                retVal.Add( await MapTile.CreateAsync( projection, x, y, cancellationToken ) );
            }
        }

        return retVal;
    }

    public IEnumerator<MapTile> GetEnumerator() => ((IEnumerable<MapTile>) _tiles).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
