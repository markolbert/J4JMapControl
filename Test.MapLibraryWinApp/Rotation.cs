using System.Globalization;

namespace Test.MapLibraryWinApp;

public record Rotation( float Value ) : SelectableItem<float>(
    Value.ToString( "{0:n0} °", CultureInfo.InvariantCulture ),
    Value );
