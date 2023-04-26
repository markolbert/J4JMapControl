using System;
using System.Linq.Expressions;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

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

        DataSourceValidator.AddRule(nameof(MapRoute.LatLongField),
                                    x => x.LatLongField,
                                    typeof(string));

        DataSourceValidator.AddRule(nameof(MapRoute.LatitudeField),
                                    x => x.LatitudeField,
                                    typeof(string));

        DataSourceValidator.AddRule(nameof(MapRoute.LongitudeField),
                                    x => x.LongitudeField,
                                    typeof(string));
    }

    protected override bool ItemIsValid(ValidationItem validationItem)
    {
        if (validationItem.ValidationResults[nameof(MapRoute.LatitudeField)] == ValidationResult.Validated
         && validationItem.ValidationResults[nameof(MapRoute.LongitudeField)] == ValidationResult.Validated)
            return true;

        return validationItem.ValidationResults[nameof(MapRoute.LatLongField)]
         == ValidationResult.Validated;
    }

    internal override IPlacedItemInternal? CreatePlacedItem( ValidationItem validationItem )
    {
        var template = _templateFunc( BindingSource );

        return template == null
            ? new PlacedElement( new Ellipse { Width = 5, Height = 5, Fill = new SolidColorBrush( Colors.Black ) } )
            : new PlacedTemplatedElement( template );
    }
}
