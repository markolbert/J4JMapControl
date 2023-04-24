using System;

namespace J4JSoftware.J4JMapWinLibrary;

public enum DataSourceValidationResult
{
    Unprocessed,
    UndefinedSource,
    SourceNotEnumerable,
    Processed
}

public enum DataItemValidationResult
{
    Untested,
    PropertyNameNotDefined,
    UndefinedDataItem,
    PropertyNotFound,
    IncorrectPropertyType,
    Validated
}
