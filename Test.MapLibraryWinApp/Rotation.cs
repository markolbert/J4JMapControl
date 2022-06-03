using System.Globalization;

namespace Test.MapLibraryWinApp;

public record Rotation( float Value ) : SelectableItem<float>(
    Value.ToString( "n0", CultureInfo.InvariantCulture ) +"°",
    Value );
