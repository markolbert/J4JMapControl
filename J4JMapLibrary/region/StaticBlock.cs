namespace J4JSoftware.J4JMapLibrary.MapRegion;

public class StaticBlock : MapBlock
{
    public static StaticBlock? CreateBlock( MapRegion region )
    {
        return region.ProjectionType switch
        {
            ProjectionType.Static => new StaticBlock
            {
                Projection = region.Projection,
                FragmentId =
                    $"{MapExtensions.LatitudeToText( region.CenterLatitude )}-{MapExtensions.LongitudeToText( region.CenterLongitude )}-{region.Scale}{GetStyleKey( region.Projection )}-{region.Projection.TileHeightWidth}-{region.Projection.TileHeightWidth}",
                CenterLatitude = region.CenterLatitude,
                CenterLongitude = region.CenterLongitude,
                Scale = region.Scale,
                Height = region.BoundingBox.Height,
                Width = region.BoundingBox.Width
            },
            _ => null
        };
    }

    public static StaticBlock? CreateBlock( IProjection projection, int xTile, int yTile, int scale )
    {
        if( projection.GetProjectionType() != ProjectionType.Static )
            return null;

        // determine the center point of the tile
        var upperLeftX = xTile * projection.TileHeightWidth;
        var upperLeftY = yTile * projection.TileHeightWidth;
        var centerPoint = new MapPoint( projection, scale );
        centerPoint.SetCartesian( upperLeftX + projection.TileHeightWidth / 2,
                                  upperLeftY + projection.TileHeightWidth / 2 );

        var heightWidth = (float) projection.TileHeightWidth;

        return new StaticBlock
        {
            Projection = projection,
            FragmentId =
                $"{MapExtensions.LatitudeToText( centerPoint.Latitude )}-{MapExtensions.LongitudeToText( centerPoint.Longitude )}-{scale}{GetStyleKey( projection )}-{projection.TileHeightWidth}-{projection.TileHeightWidth}",
            CenterLatitude = centerPoint.Latitude,
            CenterLongitude = centerPoint.Longitude,
            Scale = scale,
            Height = heightWidth,
            Width = heightWidth
        };
    }

    private StaticBlock()
    {
    }

    public float CenterLatitude { get; init; }
    public float CenterLongitude { get; init; }
}
