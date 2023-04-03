namespace J4JSoftware.J4JMapLibrary;

public record ProjectionFactoryResult( IProjection? Projection, bool ProjectionTypeFound )
{
    private readonly bool _authenticated;
    public static ProjectionFactoryResult NotFound => new( null, false );

    public bool Authenticated
    {
        get => Projection != null && _authenticated;
        init => _authenticated = value;
    }
}
