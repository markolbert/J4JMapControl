namespace J4JMapLibrary;

public record MapCreationResult( IMapProjection? Projection, bool Authenticated )
{
    public static readonly MapCreationResult NoProjection = new( null, false );
}
