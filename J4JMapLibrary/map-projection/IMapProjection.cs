using System.Collections.ObjectModel;

namespace J4JMapLibrary;

public interface IMapProjection
{
    string Name { get; }
    bool Initialized { get; }

    ProjectionMetrics Metrics { get; }

    int Width { get; }
    int Height { get; }

    Task<bool> Authenticate( string? credentials = null );
}