namespace J4JMapLibrary;

public class SourceConfiguration : ISourceConfiguration
{
    protected SourceConfiguration(
        string name
    )
    {
        Name = name;

        MaxLatitude = Math.Atan(Math.Sinh(Math.PI)) * 180 / Math.PI;
        MinLatitude = -MaxLatitude;
        MaxLongitude = 180;
        MinLongitude = -MaxLongitude;
    }

    public string Name { get; }
    public string Description { get; set; } = string.Empty;
    public string Copyright { get; set; } = string.Empty;
    public Uri? CopyrightUri { get; set; }
    public double MaxLatitude { get; set; }
    public double MinLatitude { get; set; }
    public double MaxLongitude { get; set; }
    public double MinLongitude { get; set; }
}
