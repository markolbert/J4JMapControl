namespace J4JMapLibrary;

public record TileCoordinates
{
    public TileCoordinates(
        int x,
        int y
    )
    {
        X = x < 0 ? 0 : x;
        Y = y < 0 ? 0 : y;
    }

    public int X { get; }
    public int Y { get; }
}
