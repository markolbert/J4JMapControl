﻿using System.Collections;

namespace J4JSoftware.J4JMapLibrary;

public class StaticMapRegion : MapRegion, IMapRegion
{
    public static StaticMapRegion Empty { get; } = new();

    public StaticBlock? Block { get; internal set; }
    public StaticBlock? this[ int row, int column ] => Block;

    MapBlock? IMapRegion.GetBlock( int row, int column ) => Block;

    public IEnumerator<MapBlock> GetEnumerator()
    {
        if( Block != null )
            yield return Block;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}