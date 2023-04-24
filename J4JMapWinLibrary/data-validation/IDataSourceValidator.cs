using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace J4JSoftware.J4JMapWinLibrary;

public interface IDataSourceValidator
{
    void Clear();

    void AddRule<TBindingSource>(
        string ruleName,
        object bindingSrc,
        Expression<Func<TBindingSource, string?>> propertyNameBinder,
        Type propertyType
    )
        where TBindingSource : class;

    DataSourceValidationResult Validate( object? dataSource, out List<ValidationItem> processed );
}
