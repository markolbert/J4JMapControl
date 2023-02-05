namespace J4JMapLibrary;

public partial class MapTile
{
    // ignoreCaches prevents loops when creating tiles from within the caching system
    public static async Task<MapTile> CreateAsync(
        ITiledProjection projection,
        int x,
        int y,
        CancellationToken cancellationToken,
        bool ignoreCache = false
    )
    {
        if( projection.TileCache == null || ignoreCache )
            return new MapTile( projection, x, y );

        var entry = await projection.TileCache.GetEntryAsync( projection, x, y, cancellationToken );
        return entry != null ? entry.Tile : new MapTile( projection, x, y );
    }

    public static async Task<MapTile> CreateAsync(
        ITiledProjection projection,
        Cartesian point,
        CancellationToken cancellationToken
    )
    {
        if( projection.TileCache == null )
            return new MapTile( projection, point );

        var x = point.X / projection.TileHeightWidth;
        var y = point.Y / projection.TileHeightWidth;

        var entry = await projection.TileCache.GetEntryAsync( projection, x, y, cancellationToken );
        return entry != null ? entry.Tile : new MapTile( projection, point );
    }

    public static async Task<MapTile> CreateAsync(
        ITiledProjection projection,
        LatLong latLong,
        CancellationToken cancellationToken
    )
    {
        if( projection.TileCache == null )
            return new MapTile( projection, latLong );

        var cartesianCenter = projection.LatLongToCartesian( latLong );

        var x = cartesianCenter.X / projection.TileHeightWidth;
        var y = cartesianCenter.Y / projection.TileHeightWidth;

        var entry = await projection.TileCache.GetEntryAsync( projection, x, y, cancellationToken );
        return entry != null ? entry.Tile : new MapTile( projection, latLong );
    }

    public static bool InSameProjectionScope( IProjectionScope a, IProjectionScope b ) =>
        a.Scale == b.Scale
     && Math.Abs( a.LatitudeRange.Minimum - b.LatitudeRange.Minimum ) < 0.000001
     && a.LatitudeRange.Equals( b.LatitudeRange )
     && a.LongitudeRange.Equals( b.LongitudeRange )
     && a.XRange.Equals( b.XRange )
     && a.YRange.Equals( b.YRange )
     && a.ScaleRange.Equals( b.ScaleRange );
}
