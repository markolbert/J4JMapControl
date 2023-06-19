using System.Collections;

namespace J4JSoftware.J4JMapLibrary;

public class LoadedTiledRegion : LoadedRegion, ILoadedRegion
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
                FirstRow = _blocks.Min(b => b.Row);
                LastRow = _blocks.Max(b => b.Row);

                FirstColumn = _blocks.Min(b => b.Column);
                LastColumn = _blocks.Max(b => b.Column);
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
        Blocks.FirstOrDefault( b => b.Row == row && b.Column == column )?.MapBlock;

    MapBlock? ILoadedRegion.GetBlock( int row, int column ) => this[ row, column ];

    public IEnumerator<MapBlock> GetEnumerator()
    {
        foreach( var positionedBlock in Blocks.OrderBy( b => b.Row )
                                              .ThenBy( b => b.Column ) )
        {
            yield return positionedBlock.MapBlock;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
