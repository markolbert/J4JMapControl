using J4JSoftware.J4JMapLibrary;

namespace J4JSoftware.J4JMapWinLibrary;

public record ControlViewport( string Center, string MapScale, string Heading, Viewport? Viewport )
{
    public bool IsValid => Viewport != null;
}
