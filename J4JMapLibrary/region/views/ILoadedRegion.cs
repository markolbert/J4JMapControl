using System.Numerics;
using J4JSoftware.VisualUtilities;

namespace J4JSoftware.J4JMapLibrary;

public interface ILoadedRegion
{
    bool ImagesLoaded { get; }
    float? Zoom { get; }
    Vector3 Offset { get; }

    int FirstRow { get; }
    int LastRow { get; }

    int FirstColumn { get; }
    int LastColumn { get; }

    MapBlock? GetBlock(int row, int column);
}
