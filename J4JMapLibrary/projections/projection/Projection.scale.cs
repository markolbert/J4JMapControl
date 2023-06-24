namespace J4JSoftware.J4JMapLibrary;

public abstract partial class Projection
{
    public int MinScale
    {
        get => _minScale;

        protected set
        {
            _minScale = value;
            _scaleRange = null;
        }
    }

    public int MaxScale
    {
        get => _maxScale;

        protected set
        {
            _maxScale = value;
            _scaleRange = null;
        }
    }

    public MinMax<int> ScaleRange
    {
        get
        {
            if( _scaleRange == null )
                _scaleRange = new MinMax<int>( MinScale, MaxScale );

            return _scaleRange;
        }
    }

    public MinMax<float> GetXYRange(int scale)
    {
        scale = ScaleRange.ConformValueToRange(scale, "Scale");

        var pow2 = MapExtensions.Pow(2, scale);
        return new MinMax<float>(0, TileHeightWidth * pow2 - 1);
    }

    public MinMax<int> GetTileRange(int scale)
    {
        scale = ScaleRange.ConformValueToRange(scale, "GetTileRange() Scale");
        var pow = MapExtensions.Pow(2, scale);

        return new MinMax<int>(0, pow - 1);
    }

    public int TileHeightWidth { get; protected set; }

    public int GetHeightWidth(int scale)
    {
        scale = ScaleRange.ConformValueToRange(scale, "Scale");
        var pow2 = MapExtensions.Pow(2, scale);

        return TileHeightWidth * pow2;
    }

    public int GetNumTiles(int scale)
    {
        scale = ScaleRange.ConformValueToRange(scale, "GetNumTiles()");
        return MapExtensions.Pow(2, scale);
    }

}
