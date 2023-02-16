namespace J4JMapLibrary;

public interface IMapExtract : IEnumerable<IMapFragment>
{
    bool Add( IMapFragment mapFragment );
    void Remove( IMapFragment mapFragment );
    void RemoveAt( int idx );
    void Clear();
}
