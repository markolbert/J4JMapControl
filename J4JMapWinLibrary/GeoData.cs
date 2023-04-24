namespace J4JSoftware.J4JMapWinLibrary;

public class GeoData
{
    public GeoData(
        object dataEntity
    )
    {
        DataEntity = dataEntity;
    }

    public object DataEntity { get; }
    public bool LocationIsValid { get; internal set; }

    public float Latitude { get; internal set; }
    public float Longitude { get; internal set; }
}