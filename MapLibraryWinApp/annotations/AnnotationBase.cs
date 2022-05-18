using Windows.Foundation;

namespace J4JSoftware.MapLibrary;

public abstract class AnnotationBase
{
    protected AnnotationBase(
        AnnotationType type
        )
    {
        Type = type;
    }

    public bool IsValid { get; protected set; }

    public AnnotationType Type { get; }
    public int Layer { get; set; }
    public Point Origin { get; set; }

    public abstract bool Initialize( Size clipSize, IMapProjection mapProjection );
}