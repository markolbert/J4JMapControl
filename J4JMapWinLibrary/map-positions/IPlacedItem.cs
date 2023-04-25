namespace J4JSoftware.J4JMapWinLibrary;

public interface IPlacedItem
{
    bool LocationIsValid { get; }
    float Latitude { get; }
    float Longitude { get; }
}
