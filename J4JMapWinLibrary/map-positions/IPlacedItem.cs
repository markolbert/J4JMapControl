using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public interface IPlacedItem
{
    bool LocationIsValid { get; }
    float Latitude { get; }
    float Longitude { get; }
}

public interface IPlacedElement : IPlacedItem
{
    FrameworkElement? VisualElement { get; }
}
