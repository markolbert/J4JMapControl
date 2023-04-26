using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public class PlacedElement : PlacedItem, IPlacedElement
{
    public PlacedElement(
        FrameworkElement visualElement
    )
    {
        VisualElement = visualElement;
    }

    public FrameworkElement VisualElement { get; }
}
