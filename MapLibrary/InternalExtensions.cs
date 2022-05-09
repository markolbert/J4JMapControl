namespace J4JSoftware.MapLibrary;

internal static class InternalExtensions
{
    public static double DegreesToRadians( this double degrees ) => degrees * Math.PI / 180;
    public static double RadiansToDegrees( this double radians ) => radians * 180 / Math.PI;
}
