namespace J4JMapLibrary;

public interface IBingMapServer : IMapServer<FixedMapTile, BingCredentials>
{
    BingImageryMetadata? Metadata { get; }
    BingMapType MapType { get; }
}