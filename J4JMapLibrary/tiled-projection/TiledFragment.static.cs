namespace J4JMapLibrary;

public partial class TiledFragment
{
    // ignoreCaches prevents loops when creating tiles from within the caching system
    public static async Task<TiledFragment> CreateAsync(
        ITiledProjection projection,
        int x,
        int y,
        bool ignoreCache = false,
        CancellationToken ctx = default
    )
    {
        if( projection.TileCache == null || ignoreCache )
            return new TiledFragment( projection, x, y );

        var entry = await projection.TileCache.GetEntryAsync( projection, x, y, ctx: ctx );
        return entry != null ? entry.Tile : new TiledFragment( projection, x, y );
    }
}
