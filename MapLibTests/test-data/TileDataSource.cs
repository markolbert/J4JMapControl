namespace MapLibTests;

public class TileDataSource
{
    public record Data
    (
        string ProjectionName,
        int Scale,
        float CenterLatitude,
        float CenterLongitude,
        int Height,
        int Width,
        int Heading,
        int UpperLeftX,
        int UpperLeftY,
        int LowerRightX,
        int LowerRightY
    );

    public static IEnumerable<object[]> GetTestData( string projectionName ) =>
        projectionName.Equals( "BingMaps", StringComparison.OrdinalIgnoreCase )
            ? GetOneBased( projectionName )
            : GetZeroBased( projectionName );

    private static IEnumerable<object[]> GetZeroBased( string projectionName )
    {
        yield return new object[] { new Data( projectionName, 0, 0, 0, 50, 100, 0, 0, 0, 0, 0 ) };
    }

    private static IEnumerable<object[]> GetOneBased( string projectionName )
    {
        yield return new object[] { new Data(projectionName, 1, 0, 0, 50, 100, 0, 0, 0, 0, 0) };
    }
}
