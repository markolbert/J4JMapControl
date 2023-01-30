namespace J4JMapLibrary;

public static class MapConstants
{
    public static MinMax<double> ZeroDouble = new( 0, 0 );
    public static MinMax<int> ZeroInt = new( 0, 0 );
    public static MinMax<double> AllDouble = new( double.MinValue, double.MaxValue );
    public static MinMax<int> AllInt = new( int.MinValue, int.MaxValue );
    public static MinMax<int> ViewportInt = new(0, int.MaxValue);
    public static double MaxMercatorLatitude = Math.Atan(Math.Sinh(Math.PI)) * 180 / Math.PI;

    public const double TwoPi = Math.PI * 2;
    public const double HalfPi = Math.PI / 2;
    public const double QuarterPi = Math.PI / 4;
    public const double RadiansPerDegree = Math.PI / 180;
    public const double DegreesPerRadian = 180 / Math.PI;
    public const double EarthCircumferenceMeters = 6378137;
    public const double MetersPerInch = 0.0254;
}
