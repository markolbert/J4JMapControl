namespace J4JSoftware.J4JMapLibrary;

public interface IRegionView
{
    event EventHandler<bool>? ImagesLoaded;

    IProjection Projection { get; }
    ProjectionType ProjectionType { get; }
    
    float? GetZoom( Region requestedRegion );

    Task<ILoadedRegion?> LoadRegionAsync( Region requestedRegion, CancellationToken ctx = default );
}
