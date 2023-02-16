namespace J4JMapLibrary;

public partial class MapFragments<TFrag>
{
    private record Buffer( float HeightPercent, float WidthPercent )
    {
        public virtual bool Equals( Buffer? other )
        {
            if( ReferenceEquals( null, other ) )
                return false;
            if( ReferenceEquals( this, other ) )
                return true;

            return HeightPercent.Equals( other.HeightPercent )
             && WidthPercent.Equals( other.WidthPercent );
        }

        public override int GetHashCode() => HashCode.Combine( HeightPercent, WidthPercent );

        public static readonly Buffer Default = new( 0, 0 );
    }
}
