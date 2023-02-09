namespace J4JMapLibrary;

public static class MapConstants
{
    public const float TwoPi = (float) Math.PI * 2;
    public const float HalfPi = (float) Math.PI / 2;
    public const float QuarterPi = (float) Math.PI / 4;
    public const float RadiansPerDegree = (float) Math.PI / 180;
    public const float DegreesPerRadian = (float) ( 180 / Math.PI );
    public const float EarthCircumferenceMeters = 6378137;
    public const float MetersPerInch = 0.0254F;
    public static MinMax<float> ZeroFloat = new( 0, 0 );
    public static MinMax<int> ZeroInt = new( 0, 0 );
    public static MinMax<float> AllFloat = new( float.MinValue, float.MaxValue );
    public static MinMax<int> AllInt = new( int.MinValue, int.MaxValue );
    public static MinMax<int> ViewportInt = new( 0, int.MaxValue );
    public static float Wgs84MaxLatitude = Convert.ToSingle( Math.Atan( Math.Sinh( Math.PI ) ) * 180 / Math.PI );
    public static MinMax<float> LongitudeRange = new( -180, 180 );
    public static MinMax<float> Wgs84LatitudeRange = new( -Wgs84MaxLatitude, Wgs84MaxLatitude );
}
