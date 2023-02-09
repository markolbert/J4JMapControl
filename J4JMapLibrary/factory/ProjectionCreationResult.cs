namespace J4JMapLibrary;

public record ProjectionCreationResult( IMapProjection? Projection, bool Authenticated )
{
    public static readonly ProjectionCreationResult NoProjection = new( null, false );
}