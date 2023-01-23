namespace J4JMapLibrary;

public interface IMapProjection
{
    double MaxLatitude { get; }
    double MinLatitude { get; }

    double MaxLongitude { get; }
    double MinLongitude { get; }

    int MinX { get; }
    int MaxX { get; }
    int MinY { get; }
    int MaxY { get; }
    
    int MaxScale { get; }
    int MinScale { get; }

    (double latitude, double longitude) ConvertToLatLong(int x, int y);
    (int x, int y) ConvertToXY(double latitude, double longitude);
}
