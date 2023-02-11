namespace J4JMapLibrary;

public interface IBingMapServer : IMapServer<TiledFragment, BingCredentials>
{
    BingImageryMetadata? Metadata { get; }
    BingMapType MapType { get; }
}
