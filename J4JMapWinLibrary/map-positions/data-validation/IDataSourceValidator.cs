using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace J4JSoftware.J4JMapWinLibrary;

public interface IDataSourceValidator
{
    void Clear();

    DataSourceValidationResult Validate( object? dataSource, out List<ValidationItem> processed );
}

//public interface IDataSourceValidator<TBindingSource> : IDataSourceValidator
//where TBindingSource : class
//{
//    void AddRule(
//        string ruleName,
//        Expression<Func<TBindingSource, string?>> propertyNameBinder,
//        Type propertyType
//    );
//}
