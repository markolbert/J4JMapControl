using System;

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
)
{
    private LatLong? _upperLeft;
    private LatLong? _lowerRight;

    public LatLong UpperLeft =>
        _upperLeft ??= new LatLong { Latitude = MaximumLatitude, Longitude = -MaximumLongitude };

    public LatLong LowerRight =>
        _lowerRight ??= new LatLong { Latitude = -MaximumLatitude, Longitude = MaximumLongitude };
}