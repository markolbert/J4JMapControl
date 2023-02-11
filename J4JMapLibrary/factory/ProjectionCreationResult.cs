namespace J4JMapLibrary;

public record ProjectionCreationResult( IProjection? Projection, bool Authenticated )
{
    public static readonly ProjectionCreationResult NoProjection = new( null, false );
}
