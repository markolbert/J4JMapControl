namespace J4JSoftware.J4JMapLibrary;

public abstract partial class Projection
{
    public static bool operator==( Projection? left, Projection? right ) => Equals( left, right );
    public static bool operator!=( Projection? left, Projection? right ) => !Equals( left, right );

    public bool Equals( IProjection? other ) =>
        other != null
     && MinScale == other.MinScale
     && MaxScale == other.MaxScale
     && Math.Abs( MinLatitude - other.MinLatitude ) < MapConstants.FloatTolerance
     && Math.Abs( MaxLatitude - other.MaxLatitude ) < MapConstants.FloatTolerance
     && Math.Abs( MinLongitude - other.MinLongitude ) < MapConstants.FloatTolerance
     && Math.Abs( MaxLongitude - other.MaxLongitude ) < MapConstants.FloatTolerance
     && Name.Equals( other.Name, StringComparison.OrdinalIgnoreCase );

    public override bool Equals( object? obj )
    {
#pragma warning disable IDE0041
        if( ReferenceEquals( null, obj ) )
#pragma warning restore IDE0041
            return false;
        if( ReferenceEquals( this, obj ) )
            return true;
        if( obj.GetType() != GetType() )
            return false;

        return Equals( (IProjection) obj );
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add( MinScale );
        hashCode.Add( MaxScale );
        hashCode.Add( MinLatitude );
        hashCode.Add( MaxLatitude );
        hashCode.Add( MinLongitude );
        hashCode.Add( MaxLongitude );
        hashCode.Add( Name );

        return hashCode.ToHashCode();
    }
}
