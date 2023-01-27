namespace J4JMapLibrary;

public static class MapConstants
{
    public static MinMax<double> DefaultDouble = new(0, 0);
    public static MinMax<int> DefaultInt = new(0, 0);

    public const double TwoPi = Math.PI * 2;
    public const double HalfPi = Math.PI / 2;
    public const double QuarterPi = Math.PI / 4;
    public const double RadiansPerDegree = Math.PI / 180;
    public const double DegreesPerRadian = 180 / Math.PI;
    public const double EarthCircumferenceMeters = 6378137;
    public const double MetersPerInch = 0.0254;
}
