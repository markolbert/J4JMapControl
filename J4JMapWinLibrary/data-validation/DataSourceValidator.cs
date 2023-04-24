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
    private readonly List<DataSourceValidationRule> _rules = new();
    private readonly TBindingSource _bindingSource;
    private readonly ILogger? _logger;

    public DataSourceValidator(
        TBindingSource bindingSource,
        ILoggerFactory? loggerFactory
    )
    {
        _bindingSource = bindingSource;
        _logger = loggerFactory?.CreateLogger<DataSourceValidator<TBindingSource>>();
    }

    public void Clear() => _rules.Clear();

    public void AddRule(
        Expression<Func<TBindingSource, string?>> propertyNameBinder,
        Type propertyType
    )
    {
        var propNameRetriever = propertyNameBinder.Compile();

        _rules.Add( new DataSourceValidationRule( x => propNameRetriever( (TBindingSource) x ),
                                                  propertyType ) );
    }

    public DataSourceValidationResult Validate( object? source, string sourceName, out List<object> validItems )
    {
        validItems = new List<object>();

        if( source == null )
        {
            _logger?.LogWarning( "{sourceName} is undefined", sourceName );
            return DataSourceValidationResult.UndefinedSourceName;
        }

        if( source is not IEnumerable tempEnumerable )
        {
            _logger?.LogWarning( "{sourceName} is not an {enumerable}", sourceName, typeof( IEnumerable ) );
            return DataSourceValidationResult.SourceNotEnumerable;
        }

        var enumerableSource = tempEnumerable.Cast<object?>().ToList();

        var retVal = DataSourceValidationResult.Success;

        for( var ruleNum = 0; ruleNum < _rules.Count; ruleNum++ )
        {
            var rule = _rules[ ruleNum ];

            var propName = rule.GetPropertyName( _bindingSource );
            if( string.IsNullOrEmpty( propName ) )
            {
                _logger?.LogWarning( "Rule #{ruleNum}: {propertyName} is undefined", ruleNum, propName );
                retVal |= DataSourceValidationResult.UndefinedPropertyName;
                continue;
            }

            var itemNum = 0;

            foreach( var item in enumerableSource )
            {
                if( item == null )
                {
                    _logger?.LogWarning( "Rule #{ruleNum}, item #{itemNum}: Undefined item in {source}, skipping",
                                         ruleNum,
                                         itemNum,
                                         sourceName );
                    continue;
                }

                var propInfo = item.GetType().GetProperty( propName );
                if( propInfo == null )
                {
                    _logger?.LogWarning( "Rule #{ruleNum}, item #{itemNum}: {property} property not found",
                                         ruleNum,
                                         itemNum,
                                         propName );

                    retVal |= DataSourceValidationResult.MissingProperty;
                    continue;
                }

                if( rule.PropertyType == propInfo.PropertyType )
                {
                    validItems.Add( item );
                    continue;
                }

                _logger?.LogWarning(
                    "Rule #{ruleNum}, item #{itemNum}: property {propName} is a {incorrect} but should be a {correct}",
                    ruleNum,
                    itemNum,
                    propName,
                    propInfo.PropertyType,
                    rule.PropertyType );

                retVal |= DataSourceValidationResult.PropertyFailedTest;
            }
        }

        return retVal;
    }

    void IDataSourceValidator.AddRule<T>(
        Expression<Func<T, string?>> propertyNameBinder,
        Type propertyType
    )
    where T : class
    {
        if( typeof(T)==typeof(TBindingSource) )
        {
            var compiled = propertyNameBinder.Compile();
            _rules.Add( new DataSourceValidationRule( x => compiled( (T) x ), propertyType ) );
        }
        else
            _logger?.LogError( "Expected a {correct} but got an {incorrect}",
                               typeof( Expression<Func<TBindingSource, string?>> ),
                               propertyNameBinder.GetType() );
    }
}
