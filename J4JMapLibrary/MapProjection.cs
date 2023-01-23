using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract class MapProjection : IMapProjection
{
    protected MapProjection(
        IJ4JLogger logger
        )
    {
        Logger = logger;
        Logger.SetLoggedType( GetType() );
    }

    protected IJ4JLogger Logger { get; }

    public abstract double MaxLatitude { get; }
    public abstract double MinLatitude { get; }
    public abstract double MaxLongitude { get; }
    public abstract double MinLongitude { get;}

    public abstract int MinX { get; }
    public abstract int MaxX { get; }
    public abstract int MinY { get; }
    public abstract int MaxY { get; }

    public abstract int MaxScale { get; }
    public abstract int MinScale { get; }

#pragma warning disable CS1998
    public virtual async Task<bool> InitializeAsync()
#pragma warning restore CS1998
    {
        return true;
    }

    public bool Initialized { get; protected set; }

    public int ProjectionWidthHeight { get; private set; }

    public abstract (double latitude, double longitude) ConvertToLatLong( int x, int y );
    public abstract (int x, int y) ConvertToXY( double latitude, double longitude );
}