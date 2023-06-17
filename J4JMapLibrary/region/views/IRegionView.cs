using System.ComponentModel;
using System.Numerics;
using J4JSoftware.VisualUtilities;

namespace J4JSoftware.J4JMapLibrary;

public interface IRegionView : INotifyPropertyChanged
{
    event EventHandler<bool>? ImagesLoaded;

    IProjection Projection { get; }
    ProjectionType ProjectionType { get; }
    
    Region RequestedRegion { get; }
    Region AdjustedRegion { get; }
    bool RequestAdjusted { get; }

    MapPoint? Center { get; }
    
    Rectangle2D LoadedArea { get; }
    Vector3 LoadedAreaOffset { get; }
    int TilesHighWide { get; }

    (float X, float Y) GetUpperLeftCartesian();

    float? GetZoom( Region requestedRegion );
    //void Offset( float x, float y );

    Task<ILoadedRegion> LoadRegionAsync( Region requestedRegion, CancellationToken ctx = default );
}
