namespace J4JSoftware.J4JMapLibrary;

public interface IViewport : INormalizedViewport
{
    float Heading { get; set; }
    float Rotation { get; }
}
