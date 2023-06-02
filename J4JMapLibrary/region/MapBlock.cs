#pragma warning disable CS8618
namespace J4JSoftware.J4JMapLibrary.MapRegion;

public class MapBlock 
{
    protected static string GetStyleKey( IProjection projection ) =>
        projection.MapStyle == null
            ? string.Empty
            : $"-{projection.MapStyle.ToLower()}";

    protected MapBlock(
    )
    {
    }

    public IProjection Projection { get; init; }
    public int Scale { get; init; }

    public int X { get; init; }
    public int Y { get; init; }

    public float Height { get; init; }
    public float Width { get; init; }

    protected string StyleKey { get; init; }
    public string FragmentId { get; init; }

    public (float X, float Y) GetUpperLeftCartesian() => throw new NotImplementedException();

    public byte[]? ImageData { get; set; }
    public long ImageBytes => ImageData?.Length ?? 0;
}
