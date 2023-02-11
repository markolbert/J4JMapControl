namespace J4JMapLibrary;

public class Viewport : NormalizedViewport, IViewport
{
    private float _heading;

    public Viewport(
        IProjection projection
    )
    :base(projection)
    {
    }

    // in degrees; north is 0/360; stored as mod 360
    public float Heading
    {
        get => _heading;
        set => _heading = value % 360;
    }
}
