using System;
using System.Linq.Expressions;
using System.Reflection;

namespace J4JSoftware.J4JMapWinLibrary;

public record DataSourceValidationRule(
    Func<object, string?> GetPropertyName,
    Type PropertyType
    );