namespace J4JMapLibrary;

public class BingImageryMetadata
{
    public string Copyright { get; set; } = string.Empty;
    public string BrandLogoUri { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string StatusDescription { get; set; } = string.Empty;
    public string AuthenticationResultCode { get; set; } = string.Empty;
    public string[] ErrorDetails { get; set; } = Array.Empty<string>();
    public string TraceId { get; set; } = string.Empty;
    public BingResourceSet[] ResourceSets { get; set; }= Array.Empty<BingResourceSet>();

    public bool IsValid => ResourceSets is [{ Resources.Length: 1 }];

    public BingResource? PrimaryResource => IsValid ? ResourceSets[0].Resources[0] : null;
}
