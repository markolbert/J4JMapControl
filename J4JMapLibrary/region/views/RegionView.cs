using J4JSoftware.VisualUtilities;

namespace J4JSoftware.J4JMapLibrary;

public abstract class RegionView<TProj> : IRegionView
    where TProj : class, IProjection
{
    public event EventHandler<bool>? ImagesLoaded;

    protected RegionView(
        TProj projection,
        ProjectionType projectionType
        )
    {
        Projection = projection;
        ProjectionType = projectionType;
    }

    public IProjection Projection { get; }
    public ProjectionType ProjectionType { get; }

    public float? GetZoom( Region requestedRegion )
    {
        if( requestedRegion.CenterPoint == null )
            return null;

        var heightWidth = Projection.GetHeightWidth( requestedRegion.Scale );
        var projRectangle = new Rectangle2D( heightWidth, heightWidth, coordinateSystem: CoordinateSystem2D.Display );

        var shrinkResult = projRectangle.ShrinkToFit( requestedRegion.Area!, requestedRegion.ShrinkStyle );

        return shrinkResult.Zoom;
    }

    public abstract Task<ILoadedRegion?> LoadRegionAsync( Region requestedRegion, CancellationToken ctx = default );

    protected virtual void OnImagesLoaded( bool loaded ) => ImagesLoaded?.Invoke( this, loaded );
}