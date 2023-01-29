﻿namespace J4JMapLibrary;

public partial class MapTile
{
    public static async Task<MapTile> CreateAsync( ITiledProjection projection, int x, int y )
    {
        if( projection.TileCache == null )
            return new MapTile( projection, x, y );

        var entry = await projection.TileCache.GetEntryAsync( projection, x, y );
        return entry != null ? entry.Tile : new MapTile( projection, x, y );
    }

    public static async Task<MapTile> CreateAsync( ITiledProjection projection, Cartesian point )
    {
        if( projection.TileCache == null )
            return new MapTile( projection, point );

        var x = point.X / projection.TileHeightWidth;
        var y = point.Y / projection.TileHeightWidth;

        var entry = await projection.TileCache.GetEntryAsync( projection, x, y );
        return entry != null ? entry.Tile : new MapTile( projection, point );
    }

    public static async Task<MapTile> CreateAsync( ITiledProjection projection, MapPoint center )
    {
        if( projection.TileCache == null )
            return new MapTile( projection, center );

        var x = center.Cartesian.X / projection.TileHeightWidth;
        var y = center.Cartesian.Y / projection.TileHeightWidth;

        var entry = await projection.TileCache.GetEntryAsync( projection, x, y );
        return entry != null ? entry.Tile : new MapTile( projection, center );
    }

    public static async Task<MapTile> CreateAsync( ITiledProjection projection, LatLong latLong )
    {
        if( projection.TileCache == null )
            return new MapTile( projection, latLong );

        var center = new MapPoint( projection.Metrics )
        {
            LatLong = { Latitude = latLong.Latitude, Longitude = latLong.Longitude }
        };

        var x = center.Cartesian.X / projection.TileHeightWidth;
        var y = center.Cartesian.Y / projection.TileHeightWidth;

        var entry = await projection.TileCache.GetEntryAsync( projection, x, y );
        return entry != null ? entry.Tile : new MapTile( projection, latLong );
    }
}