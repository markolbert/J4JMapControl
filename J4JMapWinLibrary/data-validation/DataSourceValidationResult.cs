using System;

namespace J4JSoftware.J4JMapWinLibrary;

[Flags]
public enum DataSourceValidationResult
{
    Success = 0,
    UndefinedSourceName = 1 << 0,
    SourceNotEnumerable = 1 << 1,
    UndefinedPropertyName = 1 << 2,
    MissingProperty = 1 << 3,
    PropertyFailedTest = 1 << 4,
};
