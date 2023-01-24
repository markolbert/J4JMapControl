namespace J4JMapLibrary;

public interface IMapProjection
{
    double MaxLatitude { get; }
    double MinLatitude { get; }

    double MaxLongitude { get; }
    double MinLongitude { get; }

    MapPoint CreateMapPoint();
    (double latitude, double longitude) XYToLatLong(int x, int y);
    (int x, int y) LatLongToXY(double latitude, double longitude);
}
