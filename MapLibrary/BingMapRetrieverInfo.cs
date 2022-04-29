namespace J4JSoftware.MapLibrary;

public record BingMapRetrieverInfo(
    string RetrievalUrl,
    List<string> RetrievalSubdomains,
    string Description,
    string CopyrightText,
    Uri CopyrightUri,
    double MaximumLatitude,
    double MaximumLongitude,
    int MinimumZoom,
    int MaximumZoom,
    int DefaultBitmapWidthHeight
) : MapRetrieverInfo( RetrievalUrl,
                      Description,
                      CopyrightText,
                      CopyrightUri,
                      MaximumLatitude,
                      MaximumLongitude,
                      MinimumZoom,
                      MaximumZoom,
                      DefaultBitmapWidthHeight )
{
    private readonly Random _random = new();

    public string GetRandomSubdomain() => RetrievalSubdomains[ _random.Next( RetrievalSubdomains.Count ) ];
}
