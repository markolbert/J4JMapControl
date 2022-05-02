namespace J4JSoftware.MapLibrary;

public record Coordinates
{
    protected Coordinates(
        DoublePoint upperLeft,
        IZoom zoom
    )
    {
        ScreenUpperLeft = upperLeft;
        Zoom = zoom;
    }

    public DoublePoint ScreenUpperLeft { get; }
    public IZoom Zoom { get; }

    public virtual bool Equals( Coordinates? other ) =>
        !ReferenceEquals( null, other )
     && ( ReferenceEquals( this, other )
         || ScreenUpperLeft.Equals( other.ScreenUpperLeft ) );

    public override int GetHashCode() => ScreenUpperLeft.GetHashCode();
}
