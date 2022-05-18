using J4JSoftware.J4JMapControl;
using J4JSoftware.MapLibrary;

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

public record Retriever( string Name, IMapImageRetriever Item ) : SelectableItem<IMapImageRetriever>( Name, Item );
