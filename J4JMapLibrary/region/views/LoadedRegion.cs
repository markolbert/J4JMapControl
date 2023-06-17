namespace J4JSoftware.J4JMapLibrary;

public record LoadedStaticRegion( float? Zoom, StaticBlock? Block ) : ILoadedRegion
{
    public static LoadedStaticRegion Empty { get; } = new(null, null);

    public bool Succeeded => Block != null;

    public int FirstRow => 0;
    public int LastRow => 0;

    public int FirstColumn => 0;
    public int LastColumn => 0;

    public StaticBlock? this[ int row, int column ] => Block;

    MapBlock? ILoadedRegion.GetBlock(int row, int column) => Block;
}

public record LoadedTiledRegion : ILoadedRegion
{
    public static LoadedTiledRegion Empty { get; } = new( null, null );

    public LoadedTiledRegion(
        float? zoom, 
        List<PositionedMapBlock>? blocks
        )
    {
        Zoom = zoom;
        Blocks = blocks;

        if( !( blocks?.Any() ?? false ) )
            return;

        Succeeded = true;

        FirstRow = blocks.Min( b => b.Row );
        LastRow = blocks.Max(b => b.Row);
        
        FirstColumn = blocks.Min( b => b.Column );
        LastColumn = blocks.Max(b => b.Column);
    }

    public float? Zoom { get; }
    public bool Succeeded { get; }

    public int FirstRow { get; }
    public int LastRow { get; }

    public int FirstColumn { get; }
    public int LastColumn { get; }

    public List<PositionedMapBlock>? Blocks { get; }

    public TileBlock? this[ int row, int column ] =>
        Blocks?.FirstOrDefault( b => b.Row == row && b.Column == column )?.MapBlock;

    MapBlock? ILoadedRegion.GetBlock( int row, int column ) => this[ row, column ];
}
