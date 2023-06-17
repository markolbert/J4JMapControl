using System.Numerics;
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

    public StaticBlock? StaticBlock { get; private set; }

    protected override async Task<ILoadedRegion> LoadRegionInternalAsync(
        Rectangle2D requestedArea,
        CancellationToken ctx
    )
    {
        if( Center == null )
            return LoadedStaticRegion.Empty;

        var heightWidth = Projection.GetHeightWidth( RequestedRegion.Scale );
        var projRectangle = new Rectangle2D( heightWidth, heightWidth, coordinateSystem: CoordinateSystem2D.Display );
        var shrinkResult = projRectangle.ShrinkToFit( requestedArea, RequestedRegion.ShrinkStyle );

        LoadedArea = shrinkResult.Rectangle;

        StaticBlock = new StaticBlock( this );

        var loaded = await ( (StaticProjection) Projection ).LoadImageAsync( StaticBlock, ctx );
        OnImagesLoaded( loaded );

        return new LoadedStaticRegion( shrinkResult.Zoom, StaticBlock );
    }
}
