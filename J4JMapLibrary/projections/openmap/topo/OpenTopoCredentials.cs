namespace J4JSoftware.J4JMapLibrary;

[ MapCredentials( "OpenTopoMaps", typeof( OpenTopoMapsProjection ) ) ]
public class OpenTopoCredentials : IOpenMapCredentials
{
    public string UserAgent { get; set; } = string.Empty;
}
