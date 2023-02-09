namespace J4JMapLibrary;

public partial class FixedMapTile
{
    // ignoreCaches prevents loops when creating tiles from within the caching system
    public static async Task<FixedMapTile> CreateAsync(
        IFixedTileProjection projection,
        int x,
        int y,
        bool ignoreCache = false,
        CancellationToken ctx = default
    )
    {
        if( projection.TileCache == null || ignoreCache )
            return new FixedMapTile( projection, x, y );

        var entry = await projection.TileCache.GetEntryAsync( projection, x, y, ctx: ctx );
        return entry != null ? entry.Tile : new FixedMapTile( projection, x, y );
    }
}
