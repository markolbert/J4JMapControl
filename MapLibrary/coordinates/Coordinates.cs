namespace J4JSoftware.MapLibrary;

public record Coordinates
{
    protected Coordinates(
        IMapProjection mapProjection
    )
    {
        MapProjection = mapProjection;
    }

    public IMapProjection MapProjection { get; }

    public virtual bool Equals( Coordinates? other ) =>
        !ReferenceEquals( null, other );

    public override int GetHashCode() => MapProjection.GetHashCode();
}
