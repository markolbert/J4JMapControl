using System.Numerics;

namespace J4JSoftware.J4JMapLibrary;

public abstract class MapRegion
{
    public bool ImagesLoaded { get; internal set; }

    public int FirstRow { get; protected set; }
    public int LastRow { get; protected set; }

    public int FirstColumn { get; protected set; }
    public int LastColumn { get; protected set; }

    public float? Zoom { get; internal set; }
    public Vector3 Offset { get; internal set; }
}
