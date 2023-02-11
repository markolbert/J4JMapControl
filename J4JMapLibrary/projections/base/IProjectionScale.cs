namespace J4JMapLibrary;

public interface IProjectionScale : IEquatable<ProjectionScale>
{
    int Scale { get; set; }
    IMapServer MapServer { get; }
}
