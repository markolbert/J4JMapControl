using System;
using System.Collections.Generic;

namespace J4JSoftware.J4JMapWinLibrary;

public record ValidationItem( object? Item )
{
    public Dictionary<string, ValidationResult> ValidationResults { get; } =
        new( StringComparer.OrdinalIgnoreCase );
}
