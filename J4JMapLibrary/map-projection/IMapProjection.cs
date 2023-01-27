using System.Collections.ObjectModel;

namespace J4JMapLibrary;

public interface IMapProjection
{
    string Name { get; }
    bool Initialized { get; }

    double MaxLatitude { get; }
    double MinLatitude { get; }

    double MaxLongitude { get; }
    double MinLongitude { get; }

    int MinX { get; }
    int MaxX { get; }

    int MinY { get; }
    int MaxY { get; }

    int Width { get; }
    int Height { get; }

    Task<bool> Authenticate( string? credentials = null );
}