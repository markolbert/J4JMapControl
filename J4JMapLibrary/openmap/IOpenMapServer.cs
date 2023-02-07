namespace J4JMapLibrary;

public interface IOpenMapServer : IMapServer<FixedMapTile>
{
    bool Initialize( string userAgent );
}