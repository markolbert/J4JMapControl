using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapWinLibrary;

public class DataSourceValidator<TBindingSource> : IDataSourceValidator
where TBindingSource : class
{
    private readonly Dictionary<string, DataSourceValidationRule<TBindingSource>> _rules =
        new( StringComparer.OrdinalIgnoreCase );

    private readonly ILogger? _logger;

    public DataSourceValidator(
        ILoggerFactory? loggerFactory
    )
    {
        _logger = loggerFactory?.CreateLogger<DataSourceValidator<TBindingSource>>();
    }

    public void Clear() => _rules.Clear();

    public void AddRule(
        string ruleName,
        TBindingSource bindingSrc,
        Expression<Func<TBindingSource, string?>> propertyNameBinder,
        Type propertyType
    )
    {
        if( _rules.ContainsKey( ruleName ) )
            throw new ArgumentException(
                $"Duplicate {typeof( DataSourceValidationRule<TBindingSource> )} rule '{ruleName}'" );

        _rules.Add( ruleName,
                    new DataSourceValidationRule<TBindingSource>( ruleName,
                                                                  bindingSrc,
                                                                  propertyNameBinder,
                                                                  propertyType ) );
    }

    public void AddRule( DataSourceValidationRule<TBindingSource> rule )
    {
        if (_rules.ContainsKey(rule.RuleName))
            throw new ArgumentException(
                $"Duplicate {typeof(DataSourceValidationRule<TBindingSource>)} rule '{rule.RuleName}'");

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

    void IDataSourceValidator.AddRule<T>(
        string ruleName,
        object bindingSrc,
        Expression<Func<T, string?>> propertyNameBinder,
        Type propertyType
    )
        where T : class
    {
        if( bindingSrc is TBindingSource castBindingSrc
        && propertyNameBinder is Expression<Func<TBindingSource, string?>> castBinder )
        {
            if( _rules.ContainsKey(ruleName))
                throw new ArgumentException(
                    $"Duplicate {typeof(DataSourceValidationRule<TBindingSource>)} rule '{ruleName}'");

            _rules.Add( ruleName,
                        new DataSourceValidationRule<TBindingSource>( ruleName,
                                                                      castBindingSrc,
                                                                      castBinder,
                                                                      propertyType ) );
        }
        else
            _logger?.LogError( "Expected a {correct} but got an {incorrect}",
                               typeof( Expression<Func<TBindingSource, string?>> ),
                               propertyNameBinder.GetType() );
    }
}
