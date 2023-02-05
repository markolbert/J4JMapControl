using J4JMapLibrary;

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
        float Heading,
        TileBounds TileBounds
    );

    public static IEnumerable<object[]> GetTestData( string projectionName ) =>
        projectionName.Equals( "BingMaps", StringComparison.OrdinalIgnoreCase )
            ? GetOneBased( projectionName )
            : GetZeroBased( projectionName );

    private static IEnumerable<object[]> GetZeroBased( string projectionName )
    {
        yield return new object[]
        {
            new Data( projectionName,
                      0,
                      0,
                      0,
                      128,
                      256,
                      0,
                      new TileBounds( new TileCoordinates( 0, 0 ),
                                      new TileCoordinates( 1, 1 ) ) )
        };
    }

    private static IEnumerable<object[]> GetOneBased( string projectionName )
    {
        yield return new object[]
        {
            new Data( projectionName,
                      1,
                      0,
                      0,
                      128,
                      256,
                      0,
                      new TileBounds( new TileCoordinates( 0, 0 ),
                                      new TileCoordinates( 1, 1 ) ) )
        };
    }
}
