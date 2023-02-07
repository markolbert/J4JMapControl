namespace J4JMapLibrary;

public interface IBingMapServer : IMapServer<FixedMapTile>
{
    BingImageryMetadata? Metadata { get; }
    BingMapType MapType { get; }
    
    Task<bool> InitializeAsync( BingCredentials credentials );
}