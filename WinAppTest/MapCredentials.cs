using J4JSoftware.J4JMapLibrary;

namespace WinAppTest;

public class MapCredentials
{
    public BingCredentials? BingCredentials { get; set; }
    public GoogleCredentials? GoogleCredentials { get; set; }
    public OpenStreetCredentials? OpenStreetCredentials { get; set; }
    public OpenTopoCredentials? OpenTopoCredentials { get; set; }
}
