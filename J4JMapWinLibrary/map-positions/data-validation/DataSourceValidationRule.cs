using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace J4JSoftware.J4JMapWinLibrary;

public class DataSourceValidationRule<TBindingSrc>
where TBindingSrc : class
{
    private readonly TBindingSrc _bindingSrc;
    private readonly Func<TBindingSrc, string?> _propRetriever;

    public DataSourceValidationRule(
        string ruleName,
        TBindingSrc bindingSrc,
        Expression<Func<TBindingSrc, string?>> propertyBinder,
        Type propertyType
    )
    {
        RuleName = ruleName;
        _bindingSrc = bindingSrc;
        _propRetriever = propertyBinder.Compile();
        PropertyType = propertyType;
    }

    public string RuleName { get; }
    public Type PropertyType { get; init; }

    public void ValidateDataSource(List<ValidationItem> toValidate )
    {
        toValidate.ForEach( x =>
        {
            if( x.ValidationResults.ContainsKey( RuleName ) )
                x.ValidationResults[ RuleName ] = DataItemValidationResult.Untested;
            else x.ValidationResults.Add( RuleName, DataItemValidationResult.Untested );
        });

        var propName = _propRetriever( _bindingSrc );
        if( string.IsNullOrEmpty( propName ) )
        {
            toValidate.ForEach(x =>
            {
                x.ValidationResults[ RuleName ] = DataItemValidationResult.PropertyNameNotDefined;
            });

            return;
        }

        foreach( var validationItem in toValidate )
        {
            var dataItem = validationItem.Item;
            DataItemValidationResult validationState;

            if( dataItem == null )
                validationState = DataItemValidationResult.UndefinedDataItem;
            else
            {
                var propInfo = dataItem.GetType().GetProperty( propName );

                if( propInfo == null )
                    validationState = DataItemValidationResult.PropertyNotFound;
                else
                    validationState = PropertyType == propInfo.PropertyType
                        ? DataItemValidationResult.Validated
                        : DataItemValidationResult.IncorrectPropertyType;
            }

            validationItem.ValidationResults[ RuleName ] = validationState;
        }
    }
}