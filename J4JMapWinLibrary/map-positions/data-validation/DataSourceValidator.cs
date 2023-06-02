using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace J4JSoftware.J4JMapWinLibrary;

public class DataSourceValidator<TBindingSource>
    where TBindingSource : class
{
    private readonly TBindingSource _bindingSource;

    private readonly Dictionary<string, DataSourceValidationRule<TBindingSource>> _rules =
        new( StringComparer.OrdinalIgnoreCase );

    public DataSourceValidator(
        TBindingSource bindingSource
    )
    {
        _bindingSource = bindingSource;
    }

    public void Clear() => _rules.Clear();

    public void AddRule(
        string ruleName,
        Expression<Func<TBindingSource, string?>> propertyNameBinder,
        Type propertyType
    )
    {
        if( _rules.ContainsKey( ruleName ) )
        {
            throw new ArgumentException(
                $"Duplicate {typeof( DataSourceValidationRule<TBindingSource> )} rule '{ruleName}'" );
        }

        _rules.Add( ruleName,
                    new DataSourceValidationRule<TBindingSource>( ruleName,
                                                                  _bindingSource,
                                                                  propertyNameBinder,
                                                                  propertyType ) );
    }

    public void AddRule( DataSourceValidationRule<TBindingSource> rule )
    {
        if( _rules.ContainsKey( rule.RuleName ) )
        {
            throw new ArgumentException(
                $"Duplicate {typeof( DataSourceValidationRule<TBindingSource> )} rule '{rule.RuleName}'" );
        }

        _rules.Add( rule.RuleName, rule );
    }

    public DataSourceValidationResult ValidationState { get; private set; } = DataSourceValidationResult.Unprocessed;

    public DataSourceValidationResult Validate( object? dataSource, out List<ValidationItem> processed )
    {
        processed = new List<ValidationItem>();
        ValidationState = DataSourceValidationResult.Unprocessed;

        if( dataSource == null )
        {
            ValidationState = DataSourceValidationResult.UndefinedSource;
            return ValidationState;
        }

        if( dataSource is not IEnumerable tempEnumerable )
        {
            ValidationState = DataSourceValidationResult.SourceNotEnumerable;
            return ValidationState;
        }

        processed = tempEnumerable.Cast<object?>()
                                  .Select( x => new ValidationItem( x ) )
                                  .ToList();

        foreach( var kvp in _rules )
        {
            kvp.Value.ValidateDataSource( processed );
        }

        ValidationState = DataSourceValidationResult.Processed;
        return ValidationState;
    }
}
