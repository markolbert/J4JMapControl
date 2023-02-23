namespace J4JSoftware.J4JMapLibrary;

public interface IOpenMapCredentials
{
    string UserAgent { get; set; }
}

[MapCredentials("OpenStreetMaps", typeof(OpenStreetMapsProjection))]
public class OpenStreetCredentials : IOpenMapCredentials
{
    public string UserAgent { get; set; } = string.Empty;
}

[MapCredentials("OpenTopoMaps", typeof(OpenTopoMapsProjection))]
public class OpenTopoCredentials : IOpenMapCredentials
{
    public string UserAgent { get; set; } = string.Empty;
}
