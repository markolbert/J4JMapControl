namespace J4JSoftware.MapLibrary;

public record MapRetrieverInfo(
    string RetrievalUrl,
    string Description,
    string CopyrightText,
    Uri CopyrightUri,
    double MaximumLatitude,
    double MaximumLongitude,
    int MinimumZoom,
    int MaximumZoom,
    int DefaultBitmapWidthHeight
);