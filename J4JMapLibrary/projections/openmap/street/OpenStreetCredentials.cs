namespace J4JSoftware.J4JMapLibrary;

[MapCredentials("OpenStreetMaps", typeof(OpenStreetMapsProjection))]
public class OpenStreetCredentials : IOpenMapCredentials
{
    public string UserAgent { get; set; } = string.Empty;
}