namespace J4JMapLibrary;

public class TileBounds : IEquatable<TileBounds>
{
    public TileBounds(
        TileCoordinates upperLeft, 
        TileCoordinates lowerRight
        )
    {
        this.UpperLeft = upperLeft;
        this.LowerRight = lowerRight;
    }

    public TileCoordinates UpperLeft { get; init; }
    public TileCoordinates LowerRight { get; init; }

    public bool Equals(TileBounds? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return UpperLeft.Equals(other.UpperLeft) && LowerRight.Equals(other.LowerRight);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == this.GetType() && Equals((TileBounds)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(UpperLeft, LowerRight);
    }

    public static bool operator ==(TileBounds? left, TileBounds? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(TileBounds? left, TileBounds? right)
    {
        return !Equals(left, right);
    }
}
