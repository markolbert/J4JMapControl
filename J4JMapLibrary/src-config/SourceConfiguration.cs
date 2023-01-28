namespace J4JMapLibrary;

public class SourceConfiguration : ISourceConfiguration
{
    #region Comparer

    private sealed class NameEqualityComparer : IEqualityComparer<SourceConfiguration>
    {
        public bool Equals( SourceConfiguration? x, SourceConfiguration? y )
        {
            if( ReferenceEquals( x, y ) )
                return true;
            if( ReferenceEquals( x, null ) )
                return false;
            if( ReferenceEquals( y, null ) )
                return false;
            if( x.GetType() != y.GetType() )
                return false;

            return x.Name == y.Name;
        }

        public int GetHashCode( SourceConfiguration obj )
        {
            return obj.Name.GetHashCode();
        }
    }

    public static IEqualityComparer<SourceConfiguration> DefaultComparer { get; } = new NameEqualityComparer();

    #endregion

    protected SourceConfiguration()
    {
        MaxLatitude = Math.Atan(Math.Sinh(Math.PI)) * 180 / Math.PI;
        MinLatitude = -MaxLatitude;
        MaxLongitude = 180;
        MinLongitude = -MaxLongitude;
    }

    public string Name { get; set; } = string.Empty;
    public bool CredentialsRequired { get; set; } = true;
    public string Copyright { get; set; } = string.Empty;
    public Uri? CopyrightUri { get; set; }
    public double MaxLatitude { get; set; }
    public double MinLatitude { get; set; }
    public double MaxLongitude { get; set; }
    public double MinLongitude { get; set; }
}
