using System;
using System.Linq.Expressions;
using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public class RoutePositions : MapPositions<MapRoute>
{
    private readonly Func<MapRoute, DataTemplate?> _templateFunc;

    public RoutePositions(
        string routeName,
        MapRoute route,
        Expression<Func<MapRoute, DataTemplate?>> templateBinder,
        int updateInterval = J4JMapControl.DefaultUpdateEventInterval
    )
        : base( route,
                routeName,
                updateInterval )
    {
        _templateFunc = templateBinder.Compile();

        DataSourceValidator.AddRule(nameof(LatLongProperty),
                                    x => x.LatLongField,
                                    typeof(string));

        DataSourceValidator.AddRule(nameof(LatitudeProperty),
                                    x => x.LatitudeField,
                                    typeof(string));

        DataSourceValidator.AddRule(nameof(LongitudeProperty),
                                    x => x.LongitudeField,
                                    typeof(string));
    }

    protected override bool ItemIsValid(ValidationItem validationItem)
    {
        if (validationItem.ValidationResults[nameof(LatitudeProperty)] == ValidationResult.Validated
         && validationItem.ValidationResults[nameof(LongitudeProperty)] == ValidationResult.Validated)
            return true;

        return validationItem.ValidationResults[nameof(LatLongProperty)]
         == ValidationResult.Validated;
    }

    internal override IPlacedItemInternal? CreatePlacedItem( ValidationItem validationItem )
    {
        var template = _templateFunc( BindingSource );

        return template == null
            ? null
            : new PlacedElement( template );
    }
}
