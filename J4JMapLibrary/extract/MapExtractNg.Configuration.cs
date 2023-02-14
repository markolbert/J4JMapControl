namespace J4JMapLibrary;

public partial class MapExtractNg
{
    private record Configuration(
        float Latitude,
        float Longitude,
        float Heading,
        int Scale,
        float Height,
        float Width,
        Buffer Buffer
    ) 
    {
        public virtual bool Equals( Configuration? other )
        {
            if( ReferenceEquals( null, other ) )
                return false;
            if( ReferenceEquals( this, other ) )
                return true;

            return Latitude.Equals( other.Latitude )
             && Longitude.Equals( other.Longitude )
             && Heading.Equals( other.Heading )
             && Scale == other.Scale
             && Height.Equals( other.Height )
             && Width.Equals( other.Width )
             && Buffer.Equals( other.Buffer );
        }

        public override int GetHashCode() => HashCode.Combine( Latitude, Longitude, Heading, Scale, Height, Width, Buffer );

        public static readonly Configuration Default = new( 0, 0, 0, 0, 0, 0, Buffer.Default );

        public bool IsValid( IProjection projection ) =>
            Height > 0 && Width > 0 && Scale >= projection.MapServer.MinScale && Scale <= projection.MapServer.MaxScale;

        public float Rotation => 360 - Heading;
    }
}
