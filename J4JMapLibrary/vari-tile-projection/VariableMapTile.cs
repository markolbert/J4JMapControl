namespace J4JMapLibrary;

public class VariableMapTile : MapTileBase<FixedTileScope>
{
    public VariableMapTile(
        IMapProjection projection,
        float latitude,
        float longitude,
        int height,
        int width,
        int scale
        ) 
        : base(projection)
    {
        Center = new LatLong(Scope);
        Center.SetLatLong(latitude,longitude);

        Height = height;
        Width = width;

        Scale = Scope.ScaleRange.ConformValueToRange(scale, "Scale");
    }

    protected override string TileId => $"(Lat: {Center.Latitude}, Long: {Center.Longitude}, Scale: {Scale})";

    public LatLong Center { get; }
    public int Height { get; }
    public int Width { get; }
    public int Scale { get; }

    protected override HttpRequestMessage? CreateImageRequest()
    {
        throw new NotImplementedException();
    }

    protected override Task<byte[]?> ExtractImageStreamAsync(HttpResponseMessage response)
    {
        throw new NotImplementedException();
    }
}
