namespace J4JMapLibrary;

public interface IVariableTileProjection : IMapProjection
{
    float GroundResolution( float latitude );
    string MapScale( float latitude, float dotsPerInch );
}

public interface IVariableTileProjection<out TScope> : IVariableTileProjection
    where TScope : MapScope
{
    TScope Scope { get; }
}
