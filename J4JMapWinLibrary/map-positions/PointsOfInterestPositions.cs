using System;
using System.Linq.Expressions;
using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public class PointsOfInterestPositions : MapPositions<J4JMapControl>
{
    private readonly Func<J4JMapControl, DataTemplate?> _templateFunc;

    public PointsOfInterestPositions(
        J4JMapControl mapControl,
        Expression<Func<J4JMapControl, DataTemplate?>> templateBinder,
        int updateInterval = J4JMapControl.DefaultUpdateEventInterval
    )
        : base( mapControl,
                "PointsOfInterest",
                updateInterval )
    {
        _templateFunc = templateBinder.Compile();

        DataSourceValidator.AddRule( nameof( LatLongProperty ),
                                     x => x.PoILatLong,
                                     typeof( string ) );
    }

    protected override bool ItemIsValid( ValidationItem validationItem ) =>
        validationItem.ValidationResults[ nameof( LatLongProperty ) ]
     == DataItemValidationResult.Validated;

    internal override IPlacedItemInternal? CreatePlacedItem( ValidationItem validationItem )
    {
        var template = _templateFunc( BindingSource );

        return template == null
            ? null
            : new PlacedPointOfInterest( template );
    }
}
