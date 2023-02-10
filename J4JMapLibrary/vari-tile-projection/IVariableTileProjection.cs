namespace J4JMapLibrary;

public interface IVariableTileProjection : IMapProjection
{
    float GroundResolution( float latitude );
    string MapScale( float latitude, float dotsPerInch );

    Task<List<IVariableMapTile>?> GetViewportRegionAsync(
        Viewport viewportData,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    );

    Task<VariableTileExtract?> GetViewportTilesAsync(
        Viewport viewportData,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    );
}

public interface IVariableTileProjection<out TScope> : IVariableTileProjection
    where TScope : MapScope
{
    TScope Scope { get; }
}
