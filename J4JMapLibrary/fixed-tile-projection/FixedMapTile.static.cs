namespace J4JMapLibrary;

public partial class FixedMapTile
{
    // ignoreCaches prevents loops when creating tiles from within the caching system
    public static async Task<FixedMapTile> CreateAsync(
        IFixedTileProjection projection,
        int x,
        int y,
        CancellationToken ctx,
        bool ignoreCache = false
    )
    {
        if( projection.TileCache == null || ignoreCache )
            return new FixedMapTile( projection, x, y );

        var entry = await projection.TileCache.GetEntryAsync( projection, x, y, ctx );
        return entry != null ? entry.Tile : new FixedMapTile( projection, x, y );
    }

    //public static async Task<FixedMapTile> CreateAsync(
    //    IFixedTileProjection projection,
    //    Cartesian point,
    //    CancellationToken ctx
    //)
    //{
    //    if( projection.TileCache == null )
    //        return new FixedMapTile( projection, point );

    //    var x = point.X / projection.TileHeightWidth;
    //    var y = point.Y / projection.TileHeightWidth;

    //    var entry = await projection.TileCache.GetEntryAsync( projection, x, y, ctx );
    //    return entry != null ? entry.Tile : new FixedMapTile( projection, point );
    //}

    //public static async Task<FixedMapTile> CreateAsync(
    //    IFixedTileProjection projection,
    //    LatLong latLong,
    //    CancellationToken ctx
    //)
    //{
    //    if( projection.TileCache == null )
    //        return new FixedMapTile( projection, latLong );

    //    var scope = (FixedTileScope) projection.GetScope();

    //    var cartesianCenter = scope.LatLongToCartesian( latLong );

    //    var x = cartesianCenter.X / projection.TileHeightWidth;
    //    var y = cartesianCenter.Y / projection.TileHeightWidth;

    //    var entry = await projection.TileCache.GetEntryAsync( projection, x, y, ctx );
    //    return entry != null ? entry.Tile : new FixedMapTile( projection, latLong );
    //}
}
