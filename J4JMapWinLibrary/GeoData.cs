namespace J4JSoftware.J4JMapWinLibrary;

public class GeoData
{
    public GeoData(
        object entity
    )
    {
        Entity = entity;
    }

    public object Entity { get; }
    public bool LocationIsValid { get; internal set; }

    public float Latitude { get; internal set; }
    public float Longitude { get; internal set; }

    public bool IsSequenced => SequenceId != null;
    public object? SequenceId { get; internal set; }
}