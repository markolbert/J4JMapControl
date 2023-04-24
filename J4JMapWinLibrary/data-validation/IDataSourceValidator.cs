using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace J4JSoftware.J4JMapWinLibrary;

public interface IDataSourceValidator
{
    void Clear();

    void AddRule<TBindingSource>(
        Expression<Func<TBindingSource, string?>> propertyNameBinder,
        Type propertyType
    )
        where TBindingSource : class;

    DataSourceValidationResult Validate( object? source, string sourceName, out List<object> validItems );
}
