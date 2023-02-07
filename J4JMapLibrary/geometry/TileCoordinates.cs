namespace J4JMapLibrary;

public class TileCoordinates : IEquatable<TileCoordinates>
{
    public TileCoordinates(
        int x, 
        int y
        )
    {
        this.X = x;
        this.Y = y;
    }

    public int X { get; init; }
    public int Y { get; init; }

    public bool Equals(TileCoordinates? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == this.GetType() && Equals((TileCoordinates)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public static bool operator ==(TileCoordinates? left, TileCoordinates? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(TileCoordinates? left, TileCoordinates? right)
    {
        return !Equals(left, right);
    }
}
