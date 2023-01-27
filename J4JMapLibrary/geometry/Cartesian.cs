using J4JSoftware.Logging;

namespace J4JMapLibrary;

public class Cartesian
{
    private readonly IJ4JLogger _logger;

    private int _x;
    private int _y;

    public Cartesian(
        ITiledProjection projection,
        IJ4JLogger logger
    )
    {
        Projection = projection;
        Scale = projection.Scale;

        _logger = logger;
        _logger.SetLoggedType( GetType() );
    }

    public ITiledProjection Projection { get; }
    public int Scale { get; }
    public bool ReflectsProjection => Scale == Projection.Scale;

    public int X
    {
        get => _x;

        set =>
            _x = InternalExtensions.ConformValueToRange( value,
                                                         Projection.MinX,
                                                         Projection.MaxX,
                                                         "X",
                                                         _logger );
    }

    public int Y
    {
        get => _y;

        set =>
            _y =
                InternalExtensions.ConformValueToRange( value,
                                                        Projection.MinY,
                                                        Projection.MaxY,
                                                        "Y",
                                                        _logger );
    }
}