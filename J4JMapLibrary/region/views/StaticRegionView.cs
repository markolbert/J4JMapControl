using J4JSoftware.VisualUtilities;

namespace J4JSoftware.J4JMapLibrary;

public class StaticRegionView : RegionView<StaticProjection>
{
    public StaticRegionView( 
        StaticProjection projection
    )
        : base( projection, ProjectionType.Static )
    {
    }

    public override async Task<ILoadedRegion?> LoadRegionAsync(
        Region region,
        CancellationToken ctx = default( CancellationToken )
    )
    {
        var area = region.Area;
        if( area == null )
            return null;

        var heightWidth = Projection.GetHeightWidth( region.Scale );

        var projRectangle = new Rectangle2D( heightWidth, heightWidth, coordinateSystem: CoordinateSystem2D.Display );
        var shrinkResult = projRectangle.ShrinkToFit( area, region.ShrinkStyle );

        var retVal = new LoadedStaticRegion
        {
            Zoom = shrinkResult.Zoom, Block = new StaticBlock( (StaticProjection) Projection, region )
        };

        retVal.ImagesLoaded = await ( (StaticProjection) Projection ).LoadImageAsync( retVal.Block, ctx );
        OnImagesLoaded( retVal.ImagesLoaded );

        return retVal;
    }
}
