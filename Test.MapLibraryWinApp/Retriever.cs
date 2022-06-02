using J4JSoftware.MapLibrary;

namespace Test.MapLibraryWinApp;

public record Retriever( string Name, IMapImageRetriever Item ) : SelectableItem<IMapImageRetriever>( Name, Item );
