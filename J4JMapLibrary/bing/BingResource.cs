using System.Text.Json.Serialization;

namespace J4JMapLibrary;

public class BingResource
{
    [ JsonPropertyName( "__type" ) ]
    public string Type { get; set; }

    public double[] BoundingBox { get; set; } = Array.Empty<double>();
    public int ImageHeight { get; set; }
    public int ImageWidth { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string[] ImageUrlSubdomains { get; set; } = Array.Empty<string>();
    public string VintageEnd { get; set; } = string.Empty;
    public string VintageStart { get; set; } = string.Empty;
    public int ZoomMax { get; set; }
    public int ZoomMin { get; set; }
}
