#pragma warning disable CS8618

namespace J4JSoftware.J4JMapControl.BingMetadata;

public class ImageryMetadata
{
    public string Copyright { get; set; }
    public string BrandLogoUri { get; set; }
    public int StatusCode { get; set; }
    public string StatusDescription { get; set; }
    public string AuthenticationResultCode { get; set; }
    public string[] ErrorDetails { get; set; }
    public string TraceId { get; set; }
    public ResourceSet[] ResourceSets { get; set; }

    public bool IsValid =>
        ResourceSets.Length == 1
     && ResourceSets[ 0 ].Resources.Length == 1;

    public Resource? PrimaryResource => IsValid ? ResourceSets[ 0 ].Resources[ 0 ] : null;
}