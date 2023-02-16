namespace J4JMapLibrary;

public class StaticBounds : IEquatable<StaticBounds>
{
    #region IEquatable...

    public bool Equals( StaticBounds? other )
    {
        if( ReferenceEquals( null, other ) )
            return false;
        if( ReferenceEquals( this, other ) )
            return true;

        return CenterLatitude.Equals( other.CenterLatitude )
         && CenterLongitude.Equals( other.CenterLongitude )
         && Height.Equals( other.Height )
         && Width.Equals( other.Width );
    }

    public override bool Equals( object? obj )
    {
        if( ReferenceEquals( null, obj ) )
            return false;
        if( ReferenceEquals( this, obj ) )
            return true;

        return obj.GetType() == GetType() && Equals( (StaticBounds) obj );
    }

    public override int GetHashCode() => HashCode.Combine( CenterLatitude, CenterLongitude, Height, Width );

    public static bool operator==( StaticBounds? left, StaticBounds? right ) => Equals( left, right );

    public static bool operator!=( StaticBounds? left, StaticBounds? right ) => !Equals( left, right );

    #endregion

    public StaticBounds(
        IStaticFragment mapTile
    )
    {
        CenterLatitude = mapTile.Center.Latitude;
        CenterLongitude = mapTile.Center.Longitude;
        Height = mapTile.Height;
        Width = mapTile.Width;
    }

    public float CenterLatitude { get; }
    public float CenterLongitude { get; }
    public float Height { get; }
    public float Width { get; }
}
