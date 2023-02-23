namespace J4JSoftware.J4JMapLibrary;

public record ProjectionFactoryResult( IProjection? Projection, bool ProjectionTypeFound, object? Credentials = null )
{
    public static ProjectionFactoryResult NotFound => new( null, false );

    private readonly bool _authenticated;

    public bool Authenticated
    {
        get => Projection != null && _authenticated;
        init => _authenticated = value;
    }

    public bool HasCredentials => Credentials != null;
}
