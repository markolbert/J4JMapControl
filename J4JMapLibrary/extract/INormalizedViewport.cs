namespace J4JMapLibrary;

public interface INormalizedViewport
{
    float CenterLatitude { get; set; }
    float CenterLongitude { get; set; }
    float Height { get; set; }
    float Width { get; set; }
    int Scale { get; set; }
}

public interface IViewport : INormalizedViewport
{
    float Heading { get; set; }
}
