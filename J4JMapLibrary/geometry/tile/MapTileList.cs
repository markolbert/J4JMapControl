using System.Runtime.CompilerServices;

namespace J4JMapLibrary;

public class MapTileList
{
    private readonly List<MapTile> _tiles = new();

    public bool Add( MapTile mapTile )
    {
        if( _tiles.Count == 0 )
        {
            _tiles.Add( mapTile );
            return true;
        }

        if( _tiles[ 0 ] != mapTile )
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

    public bool TryGetBounds( out TileBounds? bounds )
    {
        bounds = null;

        if( _tiles.Count == 0 )
            return false;

        var minX = _tiles.Min( x => x.X );
        var maxX = _tiles.Max( x => x.X );
        var minY = _tiles.Min( x => x.Y );
        var maxY = _tiles.Max( x => x.Y );

        bounds = new TileBounds( new TileCoordinates( minX, minY ), new TileCoordinates( maxX, maxY ) );

        return true;
    }

    public async IAsyncEnumerable<MapTile> GetTilesAsync(
        ITiledProjection projection,
        [ EnumeratorCancellation ] CancellationToken cancellationToken
    )
    {
        if( !TryGetBounds( out var bounds ) )
            yield break;

        for( var x = bounds!.UpperLeft.X; x <= bounds.LowerRight.X; x++ )
        {
            for( var y = bounds.UpperLeft.Y; y <= bounds.LowerRight.Y; y++ )
            {
                yield return await MapTile.CreateAsync( projection, x, y, cancellationToken );
            }
        }
    }
}
