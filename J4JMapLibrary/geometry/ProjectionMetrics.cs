using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JMapLibrary;

public record ProjectionMetrics
{
    public MinMax<double> LatitudeRange { get; init; } = MapConstants.DefaultDouble;
    public MinMax<double> LongitudeRange { get; init; } = MapConstants.DefaultDouble;
    public MinMax<int> XRange { get; init; } = MapConstants.DefaultInt;
    public MinMax<int> YRange { get; init; } = MapConstants.DefaultInt;
    public MinMax<int> ScaleRange { get; init; } = MapConstants.DefaultInt;
    public int Scale { get; init; }
    public MinMax<int> TileXRange { get; init; } = MapConstants.DefaultInt;
    public MinMax<int> TileYRange { get; init; } = MapConstants.DefaultInt;

}
