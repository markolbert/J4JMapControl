using System;
using System.Collections.Generic;

namespace J4JSoftware.J4JMapWinLibrary;

public record ValidationItem( object? DataItem )
{
    public Dictionary<string, ValidationResult> ValidationResults { get; } =
        new( StringComparer.OrdinalIgnoreCase );
}
