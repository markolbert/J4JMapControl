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

    int Width { get; }
    int Height { get; }

    MapProjection.MapPoint CreateMapPoint();
    MapProjection.LatLong CartesianToLatLong(int x, int y);
    MapProjection.Cartesian LatLongToCartesian(double latitude, double longitude);
}