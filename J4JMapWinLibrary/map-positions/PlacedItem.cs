namespace J4JSoftware.J4JMapWinLibrary;

public class PlacedItem : IPlacedItem, IPlacedItemInternal
{
    public bool LocationIsValid { get; private set; }

    public float Latitude { get; private set; }
    public float Longitude { get; private set; }

    protected virtual void Initialize( object data )
    {
    }

    void IPlacedItemInternal.Initialize( object data, float latitude, float longitude, bool isValid )
    {
        LocationIsValid = isValid;
        Latitude = latitude;
        Longitude = longitude;

        Initialize( data );
    }
}
