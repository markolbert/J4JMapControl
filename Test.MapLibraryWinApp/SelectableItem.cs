using J4JSoftware.J4JMapControl;

namespace Test.MapLibraryWinApp;

public record SelectableItem<TItem>
{
    protected SelectableItem(
        string name,
        TItem item
    )
    {
        Name = name;
        Item = item;
    }

    public string Name { get; }
    public TItem Item { get; }
}