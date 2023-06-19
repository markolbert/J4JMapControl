using J4JSoftware.VisualUtilities;
#pragma warning disable CS8618

namespace J4JSoftware.J4JMapLibrary;

public class MapBlock
{
    protected static string GetStyleKey( IProjection projection ) =>
        projection.MapStyle == null
            ? string.Empty
            : $"-{projection.MapStyle.ToLower()}";

    private string? _styleKey;

    protected MapBlock(
        IProjection projection,
        int scale
        )
    {
        Projection = projection ;
        Scale = projection.ScaleRange.ConformValueToRange( scale, "MapBlock" );
    }

    public IProjection Projection { get; }
    public int Scale { get; }

    public Rectangle2D Bounds { get; init; }

    public float Height { get; init; }
    public float Width { get; init; }

    protected string StyleKey
    {
        get
        {
            _styleKey ??= GetStyleKey( Projection );
            return _styleKey;
        }
    }

    public string FragmentId { get; init; }

    public byte[]? ImageData { get; set; }
    public long ImageBytes => ImageData?.Length ?? 0;
}
