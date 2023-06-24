using System.Collections;

namespace J4JSoftware.J4JMapLibrary;

public class TiledMapRegion : MapRegion, IMapRegion
{
    private List<PositionedMapBlock> _blocks = new();

    public List<PositionedMapBlock> Blocks
    {
        get=> _blocks;

        internal set
        {
            _blocks = value;

            if( _blocks.Any() )
            {
                FirstRow = _blocks.Min(b => b.RegionRow);
                LastRow = _blocks.Max(b => b.RegionRow);

                FirstColumn = _blocks.Min(b => b.RegionColumn);
                LastColumn = _blocks.Max(b => b.RegionColumn);
            }
            else
            {
                FirstRow = 0;
                LastRow = -1;

                FirstColumn = 0;
                LastColumn = -1;
            }
        }
    }

    public TileBlock? this[ int row, int column ] =>
        Blocks.FirstOrDefault( b => b.RegionRow == row && b.RegionColumn == column )?.MapBlock;

    MapBlock? IMapRegion.GetBlock( int row, int column ) => this[ row, column ];

    public IEnumerator<MapBlock> GetEnumerator()
    {
        foreach( var positionedBlock in Blocks.OrderBy( b => b.RegionRow )
                                              .ThenBy( b => b.RegionColumn ) )
        {
            yield return positionedBlock.MapBlock;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
