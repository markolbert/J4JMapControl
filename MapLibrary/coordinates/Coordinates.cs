namespace J4JSoftware.MapLibrary;

public record Coordinates
{
    protected Coordinates(
        DoublePoint upperLeft,
        IZoom zoom
    )
    {
        PixelUpperLeft = upperLeft;
        Zoom = zoom;
    }

    public DoublePoint PixelUpperLeft { get; }
    public IZoom Zoom { get; }

    public virtual bool Equals( Coordinates? other ) =>
        !ReferenceEquals( null, other )
     && ( ReferenceEquals( this, other )
         || PixelUpperLeft.Equals( other.PixelUpperLeft ) );

    public override int GetHashCode() => PixelUpperLeft.GetHashCode();
}
