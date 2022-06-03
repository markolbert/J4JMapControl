using System.Text.Json.Serialization;

namespace J4JSoftware.MapLibrary.BingMetadata;

#pragma warning disable CS8618
public class Resource
{
    [JsonPropertyName("__type")]
    public string Type { get; set; }

    public double[] BoundingBox { get; set; }
    public int ImageHeight { get; set; }
    public int ImageWidth { get; set; }
    public string ImageUrl { get; set; }
    public string[] ImageUrlSubdomains { get; set; }
    public string VintageEnd { get; set; }
    public string VintageStart { get; set; }
    public int ZoomMax { get; set; }
    public int ZoomMin { get; set; }
}
