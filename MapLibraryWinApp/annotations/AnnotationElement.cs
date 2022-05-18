using Microsoft.UI.Xaml;

namespace J4JSoftware.MapLibrary;

public record AnnotationElement(UIElement Element)
{
    public AnnotationBase? AnnotationInfo { get; } = MapProperties.GetAnnotationProperty( Element );
}